#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDALVQ;
using Lokad.Cloud;
using Lokad.Cloud.Storage;
using Lokad.Cloud.Runtime;
using NUnit.Framework;

namespace CloudDALVQTests
{
    [TestFixture]
    public class ComparingVersion
    {
        //Code used to compare the run with multithread and monothread.
        //[Test]
        //public void Run()
        //{
        //    var providers = Standalone.CreateMockProviders();
        //    var runTimeProviders = new RuntimeProviders(providers.BlobStorage, providers.QueueStorage,
        //                                                providers.TableStorage, providers.RuntimeFinalizer,
        //                                                providers.Log);
        //    const int p = 1;
        //    const int n = 10;
        //    const int d = 10;
        //    const int k = 7;
        //    const int g = 7;
        //    const int knotCount = 4;
        //    const double stepConstant = 1.0;
        //    const int batchSize = 1;
        //    const int evaluationCount = 100000;
        //    var additionalTimeForConsensus = new TimeSpan(0, 0,0,15);//Wait for the end of maxLoops

        //    var expiration = DateTimeOffset.Now.AddMinutes(2);
        //    var settings = new Settings(n, d, p, k, g, knotCount, stepConstant, batchSize, 2, 2,
        //        evaluationCount, expiration, additionalTimeForConsensus, GeneratorType.UniformInHyperCube);

        //    var asyncSetupService = new AsyncSetupService { Providers = providers, RuntimeProviders = runTimeProviders };
        //    var finalMergeService = new FinalAveragingService { Providers = providers, RuntimeProviders = runTimeProviders };
        //    var partialMergeService = new PartialAveragingService { Providers = providers, RuntimeProviders = runTimeProviders };
        //    var processingService = new ProcessService { Providers = providers, RuntimeProviders = runTimeProviders };

        //    var setUpMessage = new AsyncSetupMessage(settings);
        //    var queueName = TypeMapper.GetStorageName(typeof(AsyncSetupMessage));
        //    providers.QueueStorage.Put(queueName, setUpMessage);

        //    const int maxStart = 100;

        //    for (int i = 0; i < maxStart; i++)
        //    {
        //        asyncSetupService.Start();
        //    }

        //    Action processThread = () =>
        //    {
        //        var start = DateTimeOffset.Now;
        //        while (DateTimeOffset.Now - start < new TimeSpan(0, 0, 1, 0))
        //        {
        //            processingService.Start();
        //        }
        //    };

        //    Action partialMergeThread = () =>
        //    {
        //        var start = DateTimeOffset.Now;
        //        while (DateTimeOffset.Now - start < new TimeSpan(0, 0, 1, 0))
        //        {
        //            partialMergeService.Start();
        //        }
        //    };

        //    Action finalMergeThread = () =>
        //    {
        //        var start = DateTimeOffset.Now;
        //        while (DateTimeOffset.Now - start < new TimeSpan(0, 0, 1, 0))
        //        {
        //            finalMergeService.Start();
        //        }
        //    };

        //    Parallel.Invoke(new[] { processThread, partialMergeThread, finalMergeThread });

        //    var maybe = providers.BlobStorage.GetBlob(SharedWPrototypesName.Default);

        //    Assert.That(maybe.HasValue);
        //    Console.WriteLine(maybe.Value.Prototypes[0][0]);
            
        //}
    }
}
