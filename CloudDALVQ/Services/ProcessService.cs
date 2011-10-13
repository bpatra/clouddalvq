#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CloudDALVQ.BlobNames;
using CloudDALVQ.Common;
using CloudDALVQ.DataGenerator;
using CloudDALVQ.Entities;
using CloudDALVQ.Messages;
using CloudDALVQ.Services;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Storage;
using Lokad.Cloud.Storage.Shared.Logging;

namespace CloudDALVQ
{
    /// <summary>
    /// Core logic of the DALVQ algorithm. The several instances perform CLVQ executions.
    /// Retrieve the shared version and push displacement terms using multi-threading.
    /// </summary>
    [QueueServiceSettings(AutoStart = true,
        Description = "Gradient descent service")]
    public class ProcessService : QueueService<ProcessMessage>
    {
        private const string WorkerWitnessId = "0";

        private readonly TimeSpan _timeBetweenTwoLogs = new TimeSpan(0, 0, 1, 0);

        //Keyword volatile multithread compliance for reading variable.
        //WriteBurffer is written process thread and consumed by push thread. Thus this 
        //variable prevents process thread from writting in writeBuffer while pushprocess has not complete its job.
        private volatile bool _wBufferOpen = true;

        protected override void Start(ProcessMessage message)
        {
            _wBufferOpen = true;//Reset field to its initial value. Nice catch by Matthieu 'geek' Durut.

            var readBuffer = new BlockingCollection<WPrototypes>(1);
            var writeBuffer = new BlockingCollection<WPrototypes>(1);
            var settings = BlobStorage.GetBlob(SettingsName.Default).Value;
            int groupCount = 0;

            int readSuccessCount = 0;
            int writeSuccessCount = 0;
            int readFailureCount = 0;
            int writeFailureCount = 0;

            var averagingQueueName = settings.Reducing2Layers
                                         ? PartialReduceService.PartialReduceQueueName + message.PartialId
                                         : FinalReduceService.FinalReduceQueueName;

            WPrototypes localPrototypes = null;

            Action pullSharedVersion = () =>
            {
                Log.InfoFormat("worker " + message.WorkerId +
                    ", read task started in task " + Thread.CurrentThread.ManagedThreadId);

                var previousEtag = "null etag";
                var sharedPrototypesBlobName = new SharedWPrototypesName(settings.Expiration);

                while (DateTimeOffset.Now < settings.ProcessExpiration)
                {
                    try
                    {
                        var currentEtag = BlobStorage.GetBlobEtag(sharedPrototypesBlobName);

                        //If shared prototypes has been modified since last read operation
                        if (previousEtag != currentEtag)
                        {
                            previousEtag = currentEtag;
                            var newVersion = BlobStorage.GetBlob(sharedPrototypesBlobName).Value;

                            readBuffer.Add(newVersion);
                            readSuccessCount++;
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.InfoFormat("Pulling shared version failed in worker "
                            + message.WorkerId + ", error type is " + e.GetType());
                        readFailureCount++;
                    }
                }
                readBuffer.CompleteAdding();
            };

            Action pushLocalVersion = () =>
            {
                Log.InfoFormat("worker " + message.WorkerId +
                    ", write task started in task " + Thread.CurrentThread.ManagedThreadId);

                var lastLog = DateTimeOffset.MinValue;

                int lastPushed = 0;
                try//Race condition: try/catch block.
                {
                    //Use IsAddingCompleted instead of IsCompleted since if IsAddingCompleted is true, 
                    //batchCount will not be increased anymore and we may never jump into the next if
                    while (!writeBuffer.IsAddingCompleted)
                    {
                        if (groupCount - lastPushed >= settings.PushPeriods)
                        {
                            lastPushed = groupCount;

                            //Open write buffer just before consuming it.
                            _wBufferOpen = true;

                            //Take method blocks until a new local version is available in writeBuffer.
                            //Race condition: writeBuffer.IsCompleted is false (see test above) if not an exception is thrown and caught below.
                            WPrototypes gradientToPush = writeBuffer.Take();

                            _wBufferOpen = false; //Fence closed till we need another version to push

                            try
                            {
                                var newBlobname = TemporaryBlobName<WPrototypes>.GetNew(settings.Expiration);

                                //Pushing the gradient descent term into the storage
                                PushVersion(newBlobname, gradientToPush);

                                //Informing the merging service that a new update for this worker is available.
                                var updateAvailableMessage =
                                    new UpdateAvailableMessage(newBlobname, message.PartialId, message.WorkerId);

                                Put(updateAvailableMessage, averagingQueueName);
                                writeSuccessCount++;
                            }
                            catch (Exception e)
                            {
                                if (DateTimeOffset.Now - lastLog > _timeBetweenTwoLogs)
                                {
                                    Log.InfoFormat("Pushing local version failed in worker "
                                    + message.WorkerId + ", error type is " + e);

                                    lastLog = DateTimeOffset.Now;
                                    writeFailureCount++;
                                }
                                else
                                {
                                    Log.InfoFormat("error " + e.GetType() + " in worker " + message.WorkerId);
                                    writeFailureCount++;
                                }
                            }
                        }
                        else
                        {
                            Thread.Sleep(100);//ms
                        }
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Log.InfoFormat("Race condition caught in push thread, but was expected.");
                }
            };

            Action process = () =>
            {
                Log.InfoFormat("worker " + message.WorkerId +
                    ", process task started in task " + Thread.CurrentThread.ManagedThreadId);

                var dataGenerator = DataGeneratorFactory.GetGenerator(settings, settings.Seed + message.Seed);

                int n = settings.N;

                var watch = Stopwatch.StartNew();
                var data = dataGenerator.GetData(n);

                watch.Restart();
                //do not use the same seed for init and data.
                localPrototypes = settings.SameInitialisation ? Initialization(settings, settings.Seed + 19934)
                    : Initialization(settings, settings.Seed + message.Seed + 1);

                var processor = new DisplacementProcessor();

                var localDeplacement = InitDeplacement(settings);

                if (!settings.ProcessingEnabled)
                {
                    while (!readBuffer.IsCompleted)
                    {
                        Thread.Sleep(10);

                        if (_wBufferOpen)//TODO: COMMENT relashion with pushPeriods integer.
                        {
                            WPrototypes aggregatedProtos;
                            if (readBuffer.TryTake(out aggregatedProtos))
                            {
                                //Local prototypes become aggregated prototypes + localDeplacement
                                LocalReducing(ref localPrototypes, aggregatedProtos, localDeplacement);
                            }

                            //We use TryAdd instead of Add since we could manage to try to add several times a prototype while the fence is open.
                            //This way, only the first one will succeed (collection size is 1)
                            if (writeBuffer.TryAdd(localDeplacement))
                            {
                                //we need to make a deepcopy of our local deplacement term so we we can resume processing 
                                //and we do not affect the values of the deplacement to be pushed
                                localDeplacement = WPrototypes.NewEmpty(settings.K, settings.D);
                            }
                        }
                    }
                }

                //Else we process :
                else
                {
                    while (!readBuffer.IsCompleted)
                    {
                        processor.ProcessMiniBatch(data, ref localPrototypes, ref localDeplacement, settings.MiniGroupSize);
                        Thread.Sleep(1); //used so other threads could proceed.

                        groupCount += settings.MiniGroupSize;

                        if (_wBufferOpen)//TODO: COMMENT relashion with pushPeriods integer.
                        {
                            WPrototypes aggregatedProtos;
                            if (readBuffer.TryTake(out aggregatedProtos))
                            {
                                //Local prototypes become aggregated prototypes + localDeplacement
                                LocalReducing(ref localPrototypes, aggregatedProtos, localDeplacement);
                            }

                            //We use TryAdd instead of Add since we could manage to try to add several times a prototype while the fence is open.
                            //This way, only the first one will succeed (collection size is 1)
                            if (writeBuffer.TryAdd(localDeplacement))
                            {
                                //we need to make a deepcopy of our local deplacement term so we we can resume processing 
                                //and we do not affect the values of the deplacement to be pushed
                                localDeplacement = WPrototypes.NewEmpty(settings.K, settings.D);
                            }
                        }
                    }
                }

                writeBuffer.CompleteAdding();
            };

            ////Solution 1: TPL library.
            //TPL book.
            Parallel.Invoke(new ParallelOptions { MaxDegreeOfParallelism = 5 }, new[] { process, pullSharedVersion, pushLocalVersion });

            if (message.WorkerId == WorkerWitnessId)//Summary of execution.
            {
                Log.InfoFormat("worker id : " + message.WorkerId + ", Read Success : "
                    + readSuccessCount + ", Read Failure : "
                + readFailureCount + ", Write Success : " + writeSuccessCount + ", Write Failure :"
                + writeFailureCount + ", points locally processed : "
                + localPrototypes.Affectations.Sum());
            }
        }

        protected virtual void PushVersion(TemporaryBlobName<WPrototypes> prototypesName, WPrototypes prototypesToPush)
        {
            //Pushing local version
            BlobStorage.PutBlob(prototypesName, prototypesToPush);
        }

        public static WPrototypes Initialization(Settings settings, int seed)
        {
            const int n = 20;
            var init = new double[settings.K][];
            var gen = DataGeneratorFactory.GetGenerator(settings, seed + 1);
            for (int i = 0; i < settings.K; i++)
            {
                var data = gen.GetData(n);
                init[i] = Util.Average(data);
            }

            var aff = new int[settings.K];
            return new WPrototypes { Prototypes = init, Affectations = aff };
        }

        public static WPrototypes InitDeplacement(Settings settings)
        {
            var initGradient = new double[settings.K][];
            for (int i = 0 ; i < settings.K;i++)
            {
                initGradient[i] = new double[settings.D];
            }

            var affectations = new int[settings.K];

            return new WPrototypes{Affectations = affectations, Prototypes = initGradient};
        }

        public static void LocalReducing(ref WPrototypes localVersion, WPrototypes sharedVersion, WPrototypes localDeplacement)
        {
            localVersion.Prototypes = sharedVersion.Prototypes;
  
            for (int k = 0 ; k < localDeplacement.Prototypes.Length; k++)
            {
                for (int d = 0; d < localDeplacement.Prototypes[k].Length; d++)
                {
                    localVersion.Prototypes[k][d] += localDeplacement.Prototypes[k][d];
                }
            }
        }
    }
}