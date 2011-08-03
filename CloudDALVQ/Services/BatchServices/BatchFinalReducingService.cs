#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CloudDALVQ.BlobNames;
using CloudDALVQ.Entities;
using CloudDALVQ.Handy;
using CloudDALVQ.Messages;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Storage;
using Lokad.Cloud.Storage.Shared.Logging;

namespace CloudDALVQ.Services
{
    [QueueServiceSettings(AutoStart = true,
       Description = "Service that gathers versions owned by processing workers affected to it and produces the averaged version.")]
    public class BatchFinalReducingService : QueueService<BatchFinalReducingMessage>
    {
        protected override void Start(BatchFinalReducingMessage message)
        {
            var settings = BlobStorage.GetBlob(SettingsName.Default).Value;

            int iteration = 0;
            var pointsAlreadyProcessed = new int[settings.K];

            while (iteration < settings.IterationIfBatchKMeans)
            {
                var prototypesVersions = LoadPrototypes(iteration, settings);

                var sharedVersion = BatchPartialReducingService.Merge(prototypesVersions);

                for (int k=0;k < settings.K;k++)
                {
                    pointsAlreadyProcessed[k] += sharedVersion.Affectations[k];
                }

                sharedVersion.Affectations = pointsAlreadyProcessed;

                var sharedVersionName = new WPrototypesName(settings.Expiration, "shared", iteration.ToString());
                BlobStorage.PutBlob(sharedVersionName, sharedVersion); //So mappers can retrieved the appropriate version

                iteration++;

                Log.InfoFormat("iteration " + iteration + " completed");
            }

            if (settings.EvaluationEnabled)
            {
                //Calling evaluation service so snapshots of prototypes could be evaluated
                Put(new EvaluationMessage { IsList = true });
            }
        }

        List<WPrototypes> LoadPrototypes(int iteration, Settings settings)
        {
            var blobsIdAlreadyLoaded = new List<string>();
            var versionsAlreadyLoaded = new List<WPrototypes>();

            while (true)
            {
                var versionsToLoad = PrototypesRemainingToLoad(iteration, settings, blobsIdAlreadyLoaded);

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

        static WPrototypesName[] PrototypesRemainingToLoad(int iteration, Settings settings, List<string> prototypesAlreadyLoaded)
        {
            if (settings.AveragingWith2Layers)
            {
                var partialReducerCount = Range.Array(settings.M).SliceArray((int)Math.Ceiling(Math.Sqrt(settings.M))).Length;
                var partialIds = Range.Array(partialReducerCount);

                var versionsToLoad =
                    partialIds
                    .Select(partialId => new WPrototypesName(settings.Expiration, "FinalId", "PartialId:" + partialId + ", I=" + iteration))
                    .Where(e => !prototypesAlreadyLoaded.Contains(e.ToString()))
                    .ToArray();

                return versionsToLoad;
            }
            else
            {
                var indices = Range.Array(settings.M).SliceArray((int) Math.Ceiling(Math.Sqrt(settings.M)));

                var ids = Range.Array(indices.Length).SelectMany(i => indices[i].Select(j => new {i, j}));

                var versionsToLoad = ids
                    .Select(id => new WPrototypesName(settings.Expiration, id.i.ToString(), BatchProcessService.FormatName(id.j.ToString(), iteration) ))
                    .Where(e => !prototypesAlreadyLoaded.Contains(e.ToString()))
                    .ToArray();

                return versionsToLoad;
            }
        }
    }

    [Serializable, DataContract]
    public class BatchFinalReducingMessage
    {
    }
}
