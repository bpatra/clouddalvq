#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using CloudDALVQ;
using CloudDALVQ.BlobNames;
using CloudDALVQ.DataGenerator;
using CloudDALVQ.Entities;
using CloudDALVQ.Handy;
using Lokad;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Storage;
using Lokad.Cloud.Storage.Shared.Logging;

namespace CloudDALVQ.Services
{
    /// <summary>Service responsible for the clustering implementation</summary>
    [QueueServiceSettings(AutoStart = true,
       ProcessingTimeoutSeconds = 15000)]
    public class BatchProcessService : QueueService<BatchProcessMessage>
    {
        protected override void Start(BatchProcessMessage message)
        {
            var settings = BlobStorage.GetBlob(SettingsName.Default).Value;
            var dataGenerator = DataGeneratorFactory.GetGenerator(settings, settings.Seed + message.Seed);

            int n = settings.N;
            var data = dataGenerator.GetData(n);

            var prototypes = settings.SameInitialisation ? ProcessService.Initialization(settings, settings.Seed + 19934)
                    : ProcessService.Initialization(settings, settings.Seed + message.Seed + 1);

            int iteration = 0;

            //HACK : change the condition below
            while (iteration < settings.IterationBatchKMeans)
            {
                prototypes = Process(data, prototypes);

                var prototypesName = new WPrototypesName(settings.Expiration, message.PartialId, FormatName(message.WorkerId, iteration));
                BlobStorage.PutBlob(prototypesName, prototypes);

                //Synchronisation barrier and loading new shared version
                var sharedVersion = WaitForIterationToComplete(settings, iteration);

                prototypes.Prototypes = sharedVersion.Prototypes;
                prototypes.Affectations = new int[settings.K];

                iteration++;
            }
        }

        public static WPrototypes Process(double[][] points, WPrototypes oldWPrototypes)
        {
            var oldPrototypes = oldWPrototypes.Prototypes;
            var N = points.Length;
            var K = oldPrototypes.Length;
            var D = points[0].Length;

            var bestIndices = new int[points.Length];
            var newPrototypes = new double[K][];
            for (int k = 0; k < K; k++)
            {
                newPrototypes[k] = new double[D];
            }
            var affs = new int[K];
            //Array.Copy(oldWPrototypes.Affectations, affs, K);

            for (int n = 0; n < N; n++)
            {
                var point = points[n];
                var distanceMin = double.MaxValue;
                for (int k = 0; k < K; k++)
                {
                    var localCentroid = oldPrototypes[k];
                    var distance = 0.0;
                    for (int d = 0; d < D; d++)
                    {
                        var s = point[d] - localCentroid[d];
                        distance += s * s;
                    }
                    if (distance < distanceMin)
                    {
                        distanceMin = distance;
                        bestIndices[n] = k;
                    }
                }

                var localIndex = bestIndices[n];
                var thisCentroid = newPrototypes[localIndex];
                for (int d = 0; d < D; d++)
                {
                    thisCentroid[d] += point[d];
                }
                affs[localIndex]++;
            }

            //If branch handles void cluster assignment and avoids division by 0. No impact on performance
            var random = new Random(10);
            for (int k = 0; k < K; k++)
            {
                var weight = affs[k];
                if (weight == 0)
                {
                    newPrototypes[k] = points[random.Next(N - 1)];
                }
                else
                {
                    var localCentroid = newPrototypes[k];
                    for (int d = 0; d < D; d++)
                    {
                        localCentroid[d] /= weight;
                    }
                }
            }

            for (int k = 0; k < K;k++ )
            {
                affs[k] += oldWPrototypes.Affectations[k];
            }

            return new WPrototypes
            {
                Prototypes = newPrototypes,
                Affectations = affs
            };
        }

        private WPrototypes WaitForIterationToComplete(Settings settings, int iteration)
        {
            int attempt = 0;

            while (attempt < 1600) // HACK : [durut] about 400 seconds
            {
                var maybePrototypes = BlobStorage.GetBlob(new WPrototypesName(settings.Expiration, "shared", iteration.ToString()));
                if (maybePrototypes.HasValue)
                    return maybePrototypes.Value;
                else
                {
                    attempt++;
                    Thread.Sleep(1.Seconds());
                }
            }
            throw new TimeoutException("waiting was too long");
        }

        public static string FormatName(string workerId, int iteration)
        {
            return "ID:" + workerId + ", I=" + iteration;
        }
    }

    [Serializable, DataContract]
    public class BatchProcessMessage
    {
        /// <summary>Id refering to the partial worker (partial merging).</summary>
        [DataMember]
        public string PartialId { get; set; }

        /// <summary>Id refering to the processing worker.</summary>
        [DataMember]
        public string WorkerId { get; set; }

        /// <summary>Seed used for data generation, algorithm initialisation.</summary>
        [DataMember]
        public int Seed { get; set; }

        public BatchProcessMessage(string partialId, string workerId, int seed)
        {
            PartialId = partialId;
            WorkerId = workerId;
            Seed = seed;
        }
    }
}

