#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CloudDALVQ.BlobNames;
using CloudDALVQ.Common;
using CloudDALVQ.DataGenerator;
using CloudDALVQ;
using CloudDALVQ.Entities;
using CloudDALVQ.Messages;
using CloudDALVQ.Services;
using Lokad.Cloud.Storage;

namespace LocalProcessService
{

    //Following Joannes's advice: do not inherit a Lokad.Cloud service. 
    //We would have many difficulties (runtime troubles) copy/past of the logic of ProcessService is much more easy.
    public class LocalProcessService
    {
        MockLogger Log { get; set; }
        public LocalProcessService(IBlobStorageProvider blobStorage)
        {
            BlobStorage = blobStorage;
            Log = new MockLogger();
        }
        IBlobStorageProvider BlobStorage { get; set; }

        private const string WorkerWitnessId = "0";

        //Keyword volatile multithread compliance for reading variable.
        //WriteBurffer is written process thread and consumed by push thread. Thus this 
        //variable prevents process thread from writting in writeBuffer while pushprocess has not complete its job.
        private volatile bool _wBufferOpen = true;

        public void Start(ProcessMessage message)
        {
            _wBufferOpen = true;//Reset field to its initial value. Nice catch by Matthieu 'geek' Durut.

            var readBuffer = new BlockingCollection<WPrototypes>(1);
            var writeBuffer = new BlockingCollection<WPrototypes>(1);
            var settings = BlobStorage.GetBlob(SettingsName.Default).Value;
            int batchCount = 0;

            int readSuccessCount = 0;
            int writeSuccessCount = 0;
            int readFailureCount = 0;
            int writeFailureCount = 0;


            WPrototypes localPrototypes = null;

            Action pullSharedVersion = () =>
            {
                Log.InfoFormat("worker " + message.WorkerId +
                    ", read task started in task " + Thread.CurrentThread.ManagedThreadId);
                while (DateTimeOffset.Now < settings.ProcessExpiration)
                {
                    try
                    {
                        //TODO: add usage of etag to avoid multiple dowload of the same revision.
                        var newVersion = PullVersion(new SharedWPrototypesName(settings.Expiration));
                        readBuffer.Add(newVersion);
                        Log.InfoFormat(DateTimeOffset.Now.Second + "s" + DateTimeOffset.Now.Millisecond + " : Read Buffer filled");
                        readSuccessCount++;
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
                var prototypesName = new WPrototypesName(settings.Expiration, message.PartialId, message.WorkerId);

                int lastPushed = 0;
                try//Race condition: try/catch block.
                {
                    //Use IsAddingCompleted instead of IsCompleted since if IsAddingCompleted is true, 
                    //batchCount will not be increased anymore and we may never jump into the next if
                    while (!writeBuffer.IsAddingCompleted)
                    {
                        if (batchCount - lastPushed >= settings.PushPeriods)
                        {
                            lastPushed = batchCount;

                            //Open write buffer just before consuming it.
                            _wBufferOpen = true;

                            Log.InfoFormat(DateTimeOffset.Now.Second+"s"+ DateTimeOffset.Now.Millisecond + " : Fence opened");

                            //Take method blocks until a new local version is available in writeBuffer.
                            //Race condition: writeBuffer.IsCompleted is false (see test above) if not an exception is thrown and caught below.
                            WPrototypes prototypesToPush = writeBuffer.Take();

                            Log.InfoFormat(DateTimeOffset.Now.Second + "s" + DateTimeOffset.Now.Millisecond + " : write buffer consumed");

                            _wBufferOpen = false; //Fence closed till we need another version to push

                            Log.InfoFormat(DateTimeOffset.Now.Second + "s" + DateTimeOffset.Now.Millisecond + " : Fence closed");

                            try
                            {
                                this.PushVersion(prototypesToPush, settings);

                                //Informing the merging service that a new version for this worker is available.
                                Put(prototypesName, PartialAveragingService.PartialReduceQueueName + message.PartialId);
                                writeSuccessCount++;
                            }
                            catch (Exception e)
                            {
                                Log.InfoFormat("Pushing local version failed in worker "
                                    + message.WorkerId + ", error type is " + e.GetType());
                                writeFailureCount++;
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
                Log.InfoFormat("worker " + message.WorkerId + ", process task started in task " + Thread.CurrentThread.ManagedThreadId);
                var scheduler = new SamplingScheduler(0);
                var dataGenerator = DataGeneratorFactory.GetGenerator(settings, settings.Seed + message.Seed);

                int n = settings.N;

                var watch = Stopwatch.StartNew();
                var data = dataGenerator.GetData(n);
                Log.InfoFormat("Data generated in " + watch.Elapsed.TotalSeconds + " (s.)");

                watch.Restart();
                //do not use the same seed for init and data.
                localPrototypes = settings.SameInitialisation ? Initialization(settings, settings.Seed + 19934)
                    : Initialization(settings, settings.Seed + message.Seed + 1);
                Log.InfoFormat("prototypes generated in " + watch.Elapsed.TotalSeconds + " (s.)");

                var miniBatch = new double[settings.BatchSize][];
                var processor = new Processor();

                while (!readBuffer.IsCompleted)
                {
                    //Selecting points from local data that will be processed
                    scheduler.MakeBatch(data, ref miniBatch);

                    //Processing the new points
                    if (settings.ProcessingEnabled)
                    {
                        processor.ProcessMiniBatch(miniBatch, ref localPrototypes, batchCount, 1.0);
                        //Thread.Sleep(10);// COMMENT PURPOSE HERE !!!
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                    batchCount++;

                    //NanAnalyzer(localPrototypes);

                    if (_wBufferOpen)//TODO: COMMENT relashion with pushPeriods integer.
                    {
                        //We use TryAdd instead of Add since we could manage to try to add several times a prototype while the fence is open.
                        //This way, only the first one will succeed (collection size is 1)
                        if (writeBuffer.TryAdd(localPrototypes))
                        {
                            Log.InfoFormat(DateTimeOffset.Now.Second + "s" + DateTimeOffset.Now.Millisecond + " : write buffer filled");

                            //we need to make a deepcopy
                            //of our prototypes so we we can resume processing and we do not affect the values of the prototypes to be pushed
                            var newPrototypes = localPrototypes.Clone();
                            localPrototypes = newPrototypes;

                            //We do not use GetConsumingEnumerable here, since we don't want to wait for an item, 
                            //but only check if currently there is some item in the blocking collections
                            if (readBuffer.Any())
                            {
                                WPrototypes sharedPrototypes = readBuffer.Take();
                                Log.InfoFormat(DateTimeOffset.Now.Second + "s" + DateTimeOffset.Now.Millisecond + " : read buffer consumed");
                                processor.LocalMerge(ref localPrototypes, sharedPrototypes, settings.LocalMergeRatio);
                            }
                        }
                    }
                }
                writeBuffer.CompleteAdding();
            };

            ////Solution 1: TPL library.
            //TPL book.
            Parallel.Invoke(new ParallelOptions() { MaxDegreeOfParallelism = 5 }, new[] { process, pullSharedVersion, pushLocalVersion });

            if (message.WorkerId == WorkerWitnessId)//Summary of execution.
            {
                Log.InfoFormat("worker id : " + message.WorkerId + ", Read Success : "
                    + readSuccessCount + ", Read Failure : "
                + readFailureCount + ", Write Success : " + writeSuccessCount + ", Write Failure :"
                + writeFailureCount + ", points locally processed : "
                + localPrototypes.Affectations.Sum());
            }
        }

        protected virtual WPrototypes PullVersion(TemporaryBlobName<WPrototypes> name)
        {
            return BlobStorage.GetBlob(name).Value;
        }

        //-------------------------------------------------------untouched copy/paste----------------------------------------------------

        public WPrototypes Initialization(Settings settings, int seed)
        {
            return ProcessService.Initialization(settings, seed);
        }

        protected virtual void PushVersion(WPrototypes prototypesToPush, Settings settings)
        {
            //Pushing local version
            BlobStorage.PutBlob(new SharedWPrototypesName(settings.Expiration), prototypesToPush);
        }
        //Here the Inqueuing does not do anything.
        public void Put(object obj1, object obj2)
        {
            
        }

        public class MockLogger
        {
            public void InfoFormat(string str)
            {
                Console.WriteLine(str);
            }

            public void InfoFormat(string str, int i)
            {
                Console.WriteLine(str + i);
            }
        }

        static void NanAnalyzer(WPrototypes prototypes)
        {
             if (prototypes.Prototypes.Any(e => e.Any(a => double.IsNaN(a))))
             {
                 throw new Exception("Found Nan in prototypes");
             }
             if (prototypes.Affectations.Any(e => double.IsNaN(e)))
             {
                 throw new Exception("Found Nan in affectations");
             }
        }
    }
}
