#region This code is released under the terms of the new BSD licence.
//Authors : Fabrice Rossi, Matthieu Durut
//this projects build a clustering running on Microsoft.Azure.
//the code is build on top of the open source library Lokad.Cloud
//More information at : http://lokadcloud.codeplex.com/ or http://code.google.com/p/lokad-cloud/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lokad;
using Lokad.Cloud.Storage;
using Lokad.Cloud.ServiceFabric;
using Lokad.Quality;
using Lokad.Threading;

namespace AsyncQuantization
{
    [QueueServiceSettings(AutoStart = true, BatchSize = 1,
        Description = "Browse the storage part hosting the local prototypes version and merge them.")]
    [NoCodeCoverage, UsedImplicitly]
    public class PartialMergingService : BaseService<PartialMergingMessage>
    {
        protected override void Start(PartialMergingMessage message)
        {
            var versionsInDictionnary = new Dictionary<string, PrototypesVersion>();

            while(DateTimeOffset.Now < message.Expiration)
            {
                var aggregatedVersion = new PrototypesVersion
                {
                    Prototypes = new double[message.K][],
                    Weights = new int[message.K]
                };
                for (int i = 0; i < message.K; i++)
                {
                    aggregatedVersion.Prototypes[i] = new double[message.D];
                }

                //Ping the corresponding queue, and load up to 20 available messages.
                var versionsToLoad = QueueStorage.Get<PrototypesVersionName>("partialreducequeue" + message.GroupId, 20, 5.Minutes(), 3).ToArray();

                if (versionsToLoad.Any())
                    Log.InfoFormat("messages retrieved in partialMergingService");
                else
                {
                    Thread.Sleep(3000);
                    continue;
                }
                    
                //Load the prototypes versions that have been updated (according to the queue)
                //Loading is run in parallel using 5 threads
                //TODO : [durut] avoid to load multiple time the same blob if we have multiple message for a same blob
                //TODO: [durut] use ProtoBuf or binary formatter instead of DataContract 
                var versions = versionsToLoad.SelectInParallel(e => BlobStorage.GetBlob(e).Value, 5).ToArray();

                //Replace these versions into local version of prototypes versions
                for(int i = 0 ; i < versions.Length;i++)
                {
                    if (versionsInDictionnary.ContainsKey(versionsToLoad[i].JobId))
                    {
                        versionsInDictionnary[versionsToLoad[i].JobId] = versions[i];
                    }
                    else
                    {
                        versionsInDictionnary.Add(versionsToLoad[i].JobId, versions[i]);
                    }
                }

                //Merge the local prototypes versions
                foreach (var key in versionsInDictionnary.Keys)
                {
                    var sourceVersion = versionsInDictionnary[key];
                    for (int k = 0 ; k < message.K ;k++)
                    {
                        var weight = sourceVersion.Weights[k];
                        var prototype = sourceVersion.Prototypes[k];

                        for (int d=0; d< message.D; d++)
                        {
                            aggregatedVersion.Prototypes[k][d] += weight*prototype[d];
                        }
                        aggregatedVersion.Weights[k] += weight;
                    }
                }

                for (int k=0;k < message.K;k++)
                {
                    var weight = aggregatedVersion.Weights[k];
                    for (int d = 0 ; d < message.D ; d++)
                    {
                        aggregatedVersion.Prototypes[k][d] /= weight;
                    }
                }

                //Push it into last reduce step
                //TODO: [durut] use ProtoBuf or binary formatter instead of DataContract
                BlobStorage.PutBlob(new PrototypesVersionName(message.Expiration, "finalReduceGroup", message.GroupId), aggregatedVersion);

                //Push a message in the corresponding queue
                QueueStorage.Put( "finalmergingqueue",new PrototypesVersionName(message.Expiration, "finalReduceGroup", message.GroupId));

                QueueStorage.DeleteRange(versionsToLoad);
            }

            //Quantization is completed, cleaning the corresponding queue
            QueueStorage.Clear("partialreducequeue" + message.GroupId);
        }
    }
}
