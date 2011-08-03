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
using Lokad.Cloud.Storage;
using Lokad.Cloud.ServiceFabric;
using CloudDALVQ.Handy;
using Lokad.Cloud.Storage.Shared.Logging;
using Lokad.Cloud.Storage.Shared.Threading;

namespace CloudDALVQ.Services
{
    [QueueServiceSettings(AutoStart = true, 
        Description = "Service that gathers versions owned by processing workers affected to it and produces the averaged version.")]
    public class PartialAveragingService : QueueService<PartialAveragingMessage>
    {
        public const string PartialReduceQueueName = "partialreducequeue";
        private const int MilliSeconds = 500; //[patra]: comment purpose here !

        protected override void Start(PartialAveragingMessage message)
        {
            var dictionary = new Dictionary<string, WPrototypes>();
            var settings = BlobStorage.GetBlob(SettingsName.Default).Value;

            //logging purpose
            var downloadHistory = new Dictionary<string, int>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var stopwatch2 = new Stopwatch();

            //Adding some more time (TimeForConsensus), for the consensus to be done
            while(DateTimeOffset.Now < settings.ProcessExpiration.Add(settings.TimeForConsensus))
            {
                //Log.InfoFormat("PartialId" +  message.PartialId + " loop in {0} s", stopwatch.Elapsed.TotalSeconds);
                stopwatch.Restart();

                //Ping the corresponding queue, and load up to 20 available messages.
                //[patra]: comment magic numbers.
                var allNames = QueueStorage.Get<WPrototypesName>( PartialReduceQueueName + message.PartialId, 20, 5.Minutes(), 3).ToArray();

                //[patra]: comment purpose there !
                if (!allNames.Any())
                {
                    Thread.Sleep(MilliSeconds);
                    continue;
                }

                var refinedNames =
                    allNames.Distinct(blobName => new {GroupId = blobName.PartialId, JobId = blobName.WorkerId}).ToArray();

                //Load the prototypes versions that have been updated (according to the queue).
                //Loading is run in parallel using as many threads as blobs to load
                //Do not download several time the same blob in case of multiple message for a same blob
                
                var versions = refinedNames.SelectInParallel(e =>
                {
                    try
                    {
                        return BlobStorage.GetBlob(e);
                    }
                    catch (Exception)
                    {
                        Log.InfoFormat("Exception raised while pulling version in partial reducer " + message.PartialId);
                        return Maybe<WPrototypes>.Empty;
                    }

                }, refinedNames.Length).Where(w => w.HasValue).ToArray(p => p.Value);

                //Log.InfoFormat("PartialId" +  message.PartialId + "found {0} blobs dowload in {1} s", refinedNames.Length, stopwatch.Elapsed.TotalSeconds);
                //Replace these versions into local version of prototypes versions);
                for(int i = 0 ; i < versions.Length;i++)
                {
                    if (dictionary.ContainsKey(refinedNames[i].WorkerId))
                    {
                        dictionary[refinedNames[i].WorkerId] = versions[i];
                        downloadHistory[refinedNames[i].WorkerId]++;
                    }
                    else
                    {
                        dictionary.Add(refinedNames[i].WorkerId, versions[i]);
                        downloadHistory.Add(refinedNames[i].WorkerId, 1);
                    }
                }

                //Build the new partial version.
                var newVersion = new WPrototypes
                                     {
                                         Prototypes = new double[settings.K][],
                                         Affectations = new int[settings.K]
                                     };
                for (int i = 0; i < settings.K; i++)
                {
                    newVersion.Prototypes[i] = new double[settings.D];
                }

                //Averaging.
                for (int k = 0; k < settings.K; k++)
                {
                    var count = dictionary.Keys.Count;
                    foreach (var key in dictionary.Keys)
                    {
                        var pVersion = dictionary[key]; 
                        var prototype = pVersion.Prototypes[k];

                        for (int d=0; d< settings.D; d++)
                        {
                            newVersion.Prototypes[k][d] += prototype[d];
                        }
                        newVersion.Affectations[k] += pVersion.Affectations[k];
                    }
                    for (int d = 0; d < settings.D; d++)
                    {
                        newVersion.Prototypes[k][d] /= count;
                    }
                }
                

                try
                {
                    stopwatch2.Restart();
                    //Push it into last reduce step
                    BlobStorage.PutBlob(new WPrototypesName(settings.Expiration, FinalAveragingService.FinaleReduceGroup, message.PartialId),
                                        newVersion);

                    //Log.InfoFormat("partialId " + message.PartialId + "pushed new version using " + dictionary.Count + " versions in {0} s", stopwatch2.Elapsed.TotalSeconds);

                    //Push a message in the corresponding queue
                    QueueStorage.Put(FinalAveragingService.FinalAveragingQueueName,
                                     new WPrototypesName(settings.Expiration, FinalAveragingService.FinaleReduceGroup, message.PartialId));

                    QueueStorage.DeleteRange(allNames);
                }
                catch (Exception e)
                {
                    Log.InfoFormat("Exception raised while pushing prototypes in partial reducer " + message.PartialId);
                }
            }

            //Quantization is completed, cleaning the corresponding queue
            QueueStorage.Clear(PartialReduceQueueName + message.PartialId);

            var infosBuilder = new StringBuilder();

            foreach (var key in downloadHistory.Keys)
            {
                infosBuilder.Append("Prototype version : " + key + ", retrieved " + downloadHistory[key] +
                                    "times in partial reducer " + message.PartialId + ";\t ");
            }

            Log.InfoFormat(infosBuilder.ToString());
        }
    }
}
