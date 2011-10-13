#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using CloudDALVQ;
using CloudDALVQ.BlobNames;
using CloudDALVQ.Entities;
using CloudDALVQ.Messages;
using CloudDALVQ.Services;
using Lokad.Cloud;
using Lokad.Cloud.Mock;
using Lokad.Cloud.Runtime;
using Lokad.Cloud.Storage;
using NUnit.Framework;

namespace CloudDALVQTests
{
    [TestFixture]
    public class OverallTests
    {
        [Test]
        public void RunOverallTests()
        {
            var a = CloudStorage.ForInMemoryStorage().BuildStorageProviders();
            var providers = new CloudInfrastructureProviders(a.BlobStorage, a.QueueStorage,
                                                             a.TableStorage, new MemoryLogger(), new MemoryProvisioning(),
                                                             a.RuntimeFinalizer);
            
            var runTimeProviders = new RuntimeProviders(providers.BlobStorage, providers.QueueStorage,
                                                        providers.TableStorage, providers.RuntimeFinalizer, null);

            const int p = 1;
            const int n = 100;
            const int d = 2;
            const int k = 4;
            const int g = 7;
            const int knotCount = 4;
            const int batchSize = 50;
            const int evaluationCount = 100000;
            TimeSpan additionalTimeForConsensus = new TimeSpan(0,0,20);
            const double mergingRatio = 0.5;
            const int iterationIfBatch = 5;
            var processDuration = new TimeSpan(0,5,0);

            var settings = new Settings(n, d, p, k, g, knotCount, iterationIfBatch, batchSize,  2 ,
                evaluationCount, processDuration, additionalTimeForConsensus, GeneratorType.UniformInHyperCube, 13, false, true, true, true);

            var asyncSetupService = new AsyncSetupService {Providers = providers, RuntimeProviders = runTimeProviders};
            var finalMergeService = new FinalReduceService { Providers = providers, RuntimeProviders = runTimeProviders };
            var partialMergeService = new PartialReduceService { Providers = providers, RuntimeProviders = runTimeProviders };
            var processingService = new ProcessService { Providers = providers, RuntimeProviders = runTimeProviders };
            
            var setUpMessage = new AsyncSetupMessage(settings);
            var queueName = TypeMapper.GetStorageName(typeof(AsyncSetupMessage));
            providers.QueueStorage.Put(queueName, setUpMessage);

            const int maxStart = 100;

            for(int i = 0; i < maxStart; i++)
            {
                asyncSetupService.Start();
            }

            Action processThread1 = () =>
            {
                var start = DateTimeOffset.Now;
                while (DateTimeOffset.Now - start < new TimeSpan(0,0,1,0))
                {
                    processingService.Start();
                }
            };

            Action processThread2 = () =>
            {
                var start = DateTimeOffset.Now;
                while (DateTimeOffset.Now - start < new TimeSpan(0, 0, 1, 0))
                {
                    processingService.Start();
                }
            };

            Action processThread3 = () =>
            {
                var start = DateTimeOffset.Now;
                while (DateTimeOffset.Now - start < new TimeSpan(0, 0, 1, 0))
                {
                    processingService.Start();
                }
            };

            Action processThread4 = () =>
            {
                var start = DateTimeOffset.Now;
                while (DateTimeOffset.Now - start < new TimeSpan(0, 0, 1, 0))
                {
                    processingService.Start();
                }
            };

            Action partialMergeThread = () =>
            {
                var start = DateTimeOffset.Now;
                while (DateTimeOffset.Now - start < new TimeSpan(0, 0, 1, 0))
                {
                    partialMergeService.Start();
                }
            };

            Action finalMergeThread = () =>
            {
                var start = DateTimeOffset.Now;
                while (DateTimeOffset.Now - start < new TimeSpan(0, 0, 1, 0))
                {
                    finalMergeService.Start();
                }
            };

            Parallel.Invoke( new ParallelOptions{MaxDegreeOfParallelism = 5} ,new[] { processThread1, partialMergeThread, finalMergeThread });

            var maybe = providers.BlobStorage.GetBlob(new SharedWPrototypesName(settings.Expiration));

            Assert.That(maybe.HasValue);
            
        }

        [Test]
        public void RunOverallBatchTests()
        {
            var a = CloudStorage.ForInMemoryStorage().BuildStorageProviders();
            var providers = new CloudInfrastructureProviders(a.BlobStorage, a.QueueStorage,
                                                             a.TableStorage, new MemoryLogger(), new MemoryProvisioning(),
                                                             a.RuntimeFinalizer);

            var runTimeProviders = new RuntimeProviders(providers.BlobStorage, providers.QueueStorage,
                                                        providers.TableStorage, providers.RuntimeFinalizer, null);

            const int p = 1;
            const int n = 10000;
            const int d = 2;
            const int k = 4;
            const int g = 7;
            const int knotCount = 4;
            const int batchSize = 50;
            const int evaluationCount = 1000;
            TimeSpan additionalTimeForConsensus = new TimeSpan(0, 0, 20);
            
            const int iterationIfBatch = 5;

            var processDuration = new TimeSpan(0, 5, 0);

            var settings = new Settings(n, d, p, k, g, knotCount, iterationIfBatch, batchSize, 2,
                evaluationCount, processDuration, additionalTimeForConsensus, GeneratorType.UniformInHyperCube, 13, false, true, true, true);

            var asyncSetupService = new BatchSetUpService { Providers = providers, RuntimeProviders = runTimeProviders };
            var finalMergeService = new BatchFinalReducingService { Providers = providers, RuntimeProviders = runTimeProviders };
            var partialMergeService = new BatchPartialReducingService { Providers = providers, RuntimeProviders = runTimeProviders };
            var processingService = new BatchProcessService { Providers = providers, RuntimeProviders = runTimeProviders };

            var setUpMessage = new BatchSetupMessage(settings);
            var queueName = TypeMapper.GetStorageName(typeof(BatchSetupMessage));
            providers.QueueStorage.Put(queueName, setUpMessage);

            const int maxStart = 100;

            for (int i = 0; i < maxStart; i++)
            {
                asyncSetupService.Start();
            }

            Action processThread1 = () =>
            {
                var start = DateTimeOffset.Now;
                while (DateTimeOffset.Now - start < new TimeSpan(0, 0, 1, 0))
                {
                    processingService.Start();
                }
            };

            Action processThread2 = () =>
            {
                var start = DateTimeOffset.Now;
                while (DateTimeOffset.Now - start < new TimeSpan(0, 0, 1, 0))
                {
                    processingService.Start();
                }
            };

            Action processThread3 = () =>
            {
                var start = DateTimeOffset.Now;
                while (DateTimeOffset.Now - start < new TimeSpan(0, 0, 1, 0))
                {
                    processingService.Start();
                }
            };

            Action processThread4 = () =>
            {
                var start = DateTimeOffset.Now;
                while (DateTimeOffset.Now - start < new TimeSpan(0, 0, 1, 0))
                {
                    processingService.Start();
                }
            };

            Action partialMergeThread = () =>
            {
                var start = DateTimeOffset.Now;
                while (DateTimeOffset.Now - start < new TimeSpan(0, 0, 1, 0))
                {
                    partialMergeService.Start();
                }
            };

            Action finalMergeThread = () =>
            {
                var start = DateTimeOffset.Now;
                while (DateTimeOffset.Now - start < new TimeSpan(0, 0, 1, 0))
                {
                    finalMergeService.Start();
                }
            };

            Parallel.Invoke(new ParallelOptions() { MaxDegreeOfParallelism = 5 }, new[] { processThread1, processThread2, partialMergeThread, finalMergeThread });

            var maybe = providers.BlobStorage.GetBlob(new SharedWPrototypesName(settings.Expiration));

            Assert.That(maybe.HasValue);

        }

        [Test]
        public void BlockingCollectionTest()
        {
            var blockingcollection = new BlockingCollection<int>();
            var start = DateTimeOffset.Now;

            Action action1 = () =>
            {
                int i = 0;
                while (DateTimeOffset.Now - start < new TimeSpan(0,0,3,0))
                {
                    blockingcollection.Add(i);
                    i++;
                    Thread.Sleep(1000);
                }
            };

            Action action2 = () =>
            {
                int j = 0;

                while (DateTimeOffset.Now - start < new TimeSpan(0, 0, 3, 0))
                {
                    int lastj = 0;

                    while (blockingcollection.TryTake(out j))
                    {
                        lastj = j;
                        
                    }
                    Thread.Sleep(10000);
                }
            };

            Parallel.Invoke(new[] { action1, action2});
        }
    }
}
