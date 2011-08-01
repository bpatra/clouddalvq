#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using CloudDALVQ.BlobNames;
using CloudDALVQ.Entities;
using CloudDALVQ.Messages;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Storage;
using Lokad.Cloud.Storage.Shared.Logging;
using Lokad.Cloud.Storage.Shared.Threading;
using CloudDALVQ.Handy;

namespace CloudDALVQ.Services
{
    [QueueServiceSettings(AutoStart = true, 
        Description = "Service that gathers versions owned by PartialAveragingServices and produces the averaged version (shared version).")]
    public class FinalAveragingService : BaseService<FinalAveragingMessage>
    {
        public const string FinalAveragingQueueName = "finalmergingqueue";
        public const string FinaleReduceGroup = "FinaleReduceGroup";

        private const int milliSec = 500; //[patra]: comment purpose here !


        //HACK: [durut] Most of this method is copy/pasted from PartialReduceService.Start
        protected override void Start(FinalAveragingMessage message)
        {
            var dictionary = new Dictionary<string, WPrototypes>();
            var settings = BlobStorage.GetBlob(SettingsName.Default).Value;

            int count = 1;
            //When processing is expired, we still continue to merge for some additional 
            //time (TimeForConsensus), for the consensus to be done
            while(DateTimeOffset.Now < settings.ProcessExpiration.Add(settings.TimeForConsensus)) 
            {
                //Ping the corresponding queue, and load up to 20 available messages.
                var versionsToLoad = QueueStorage.Get<WPrototypesName>(FinalAveragingQueueName, 20, 5.Minutes(), 3).ToArray();

                if (!versionsToLoad.Any())
                {
                    Thread.Sleep(milliSec);
                    continue;
                }

                //Do not download several time the same blob in case of multiple message for a same blob
                var refinedVersion =
                    versionsToLoad.Distinct(blobName => new { GroupId = blobName.PartialId, JobId = blobName.WorkerId }).ToArray();

                //Load the prototypes versions that have been updated (according to the queue)
                //Loading is run in parallel using as many threads as there are blobs to load.
                var versions = refinedVersion.SelectInParallel(e => 
                    {
                        try
                        {
                            return BlobStorage.GetBlob(e);
                        }
                        catch (Exception)
                        {
                            Log.InfoFormat("Could not retrieve some prototypes version in final reducer");
                            return Maybe<WPrototypes>.Empty;
                        }
                    }, refinedVersion.Length).Where(w => w.HasValue).ToArray(p => p.Value);

                //Replace these versions into local version of prototypes versions
                for(int i = 0 ; i < versions.Length;i++)
                {
                    if (dictionary.ContainsKey(refinedVersion[i].WorkerId))
                    {
                        dictionary[refinedVersion[i].WorkerId] = versions[i];
                    }
                    else
                    {
                        dictionary.Add(refinedVersion[i].WorkerId, versions[i]);
                    }
                }

                //Build the new shared version.
                var newVersion = new WPrototypes
                {
                    Prototypes = new double[settings.K][],
                    Affectations = new int[settings.K]
                };
                for (int i = 0; i < settings.K; i++)
                {
                    newVersion.Prototypes[i] = new double[settings.D];
                }

                //Merge the local prototypes versions
                foreach (var key in dictionary.Keys)
                {
                    var sourceVersion = dictionary[key];
                    for (int k = 0 ; k < settings.K ;k++)
                    {
                        var prototype = sourceVersion.Prototypes[k];

                        for (int d=0; d< settings.D; d++)
                        {
                            newVersion.Prototypes[k][d] += prototype[d];
                        }
                        
                        newVersion.Affectations[k] += sourceVersion.Affectations[k];
                    }
                }

                var versionCount = dictionary.Keys.Count;
                for (int k=0;k < settings.K;k++)
                {
                    for (int d = 0 ; d < settings.D ; d++)
                    {
                        newVersion.Prototypes[k][d] /= versionCount;
                    }
                }

                try
                {
                    //Push it into last reduce step
                    BlobStorage.PutBlob(new SharedWPrototypesName(settings.Expiration), newVersion);
                    count++;
                    //Delete messages from queue
                    QueueStorage.DeleteRange(versionsToLoad);
                }
                catch (Exception e)
                {
                    Log.InfoFormat("Issue while pushing prototypes in final reducer.");
                }
            }

            Log.InfoFormat(count +": different Shared Version pushed.");

            //Quantization is completed, cleaning the corresponding queue
            QueueStorage.Clear(FinalAveragingQueueName);

            if (settings.EvaluationEnabled)
            {
                //Calling evaluation service so snapshots of prototypes could be evaluated
                Put(new EvaluationMessage { IsList = true });
            }
        }
    }
}
