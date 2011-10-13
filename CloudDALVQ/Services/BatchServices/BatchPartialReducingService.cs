#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using CloudDALVQ.BlobNames;
using CloudDALVQ.Entities;
using CloudDALVQ.Handy;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Storage;
using System.Linq;

namespace CloudDALVQ.Services
{
    [QueueServiceSettings(AutoStart = true,
       Description = "Service that gathers versions owned by processing workers affected to it and produces the averaged version.")]
    public class BatchPartialReducingService : QueueService<BatchPartialReducingMessage>
    {
        protected override void Start(BatchPartialReducingMessage message)
        {
            var settings = BlobStorage.GetBlob(SettingsName.Default).Value;

            int iteration = 0;

            //HACK : change the condition below
            while (iteration <settings.IterationBatchKMeans)
            {
                var prototypesVersions = LoadPrototypes(iteration, message.PartialId, settings);

                var aggregatedVersion = Merge(prototypesVersions);
                var aggregatedVersionName = new WPrototypesName(settings.Expiration, "FinalId", "PartialId:"+ message.PartialId+", I="+iteration );

                BlobStorage.PutBlob(aggregatedVersionName, aggregatedVersion);
                iteration++;
            }
        }

        List<WPrototypes> LoadPrototypes(int iteration, string partialId, Settings settings)
        {
            var blobsIdAlreadyLoaded = new List<string>();
            var versionsAlreadyLoaded = new List<WPrototypes>();

            while (true)
            {
                var versionsToLoad = PrototypesRemainingToLoad(partialId, iteration, settings, blobsIdAlreadyLoaded);

                if (versionsToLoad.Any())
                {
                    foreach (var versionToLoad in versionsToLoad)
                    {
                        var version = BlobStorage.GetBlob(versionToLoad);

                        if (version.HasValue)
                        {
                            versionsAlreadyLoaded.Add(version.Value);
                            blobsIdAlreadyLoaded.Add(versionToLoad.ToString());
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            return versionsAlreadyLoaded;
        }

        static WPrototypesName[] PrototypesRemainingToLoad( string partialId, int iteration, Settings settings, List<string> prototypesAlreadyLoaded)
        {
            var reducerId = int.Parse(partialId);
            var versionIdsToLoad = Range.Array(settings.M).SliceArray((int)Math.Ceiling(Math.Sqrt(settings.M)))[reducerId];

            var versionsToLoad = 
                versionIdsToLoad
                .Select(workerId => new WPrototypesName(settings.Expiration, partialId, "ID:" + workerId + ", I=" + iteration))
                .Where(e => !prototypesAlreadyLoaded.Contains(e.ToString()))
                .ToArray();

            return versionsToLoad;
        }

        public static WPrototypes Merge(List<WPrototypes> localPrototypes)
        {
            var protos = localPrototypes.Select(e => e.Prototypes).ToArray();
            var affs = localPrototypes.Select(e => e.Affectations).ToArray();
            var P = localPrototypes.Count; //Here, P is not the number of workers but of partial results retrieved
            var K = protos[0].Length;
            var D = protos[0][0].Length;
            var mergedProtos = Range.Array(K).Select(i => new double[D]).ToArray();
            var mergedAffs = new int[K];

            //Probably the optimal way to run the loops while optimising cache memory access
            //TODO: [durut] to get even better performance, we should break the loop since float addition takes 1 cycle for computation but has 3 cycles of latency 
            for (int p = 0; p < P; p++)
            {
                for (int k = 0; k < K; k++)
                {
                    for (int d = 0; d < D; d++)
                    {
                        mergedProtos[k][d] += affs[p][k] * protos[p][k][d];
                    }
                    mergedAffs[k] += affs[p][k];
                }
            }

            for (int k = 0; k < K;k++ )
            {
                for (int d =0; d < D; d++)
                {
                    mergedProtos[k][d] /= mergedAffs[k];
                }
            }

            return new WPrototypes()
            {
                Prototypes = mergedProtos,
                Affectations = mergedAffs
            };
        }
    }

    [Serializable, DataContract]
    public class BatchPartialReducingMessage
    {
        /// <summary></summary>
        [DataMember]
        public string PartialId { get; set; }

        public BatchPartialReducingMessage(string partialId)
        {
            PartialId = partialId;
        }
    }
}
