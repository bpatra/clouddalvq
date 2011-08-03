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
using Lokad.Cloud.Storage.Shared.Logging;
using Lokad.Cloud.Storage.Shared.Threading;
using CloudDALVQ.Handy;

namespace CloudDALVQ.Services
{
    [QueueServiceSettings(AutoStart = true,
       Description = "Service that gathers versions owned by processing workers affected to it and produces the averaged version.")]
    public class PartialReduceService : QueueService<PartialReducingMessage>
    {
        public const string PartialReduceQueueName = "partialreducegradientqueue";
        private const int MilliSeconds = 100; //[patra]: comment purpose here !

        protected override void Start(PartialReducingMessage message)
        {
            var settings = BlobStorage.GetBlob(SettingsName.Default).Value;

            //logging purpose
            var downloadHistory = new Dictionary<string, int>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var stopwatch2 = new Stopwatch();

            var sumOfUpdates = WPrototypes.NewEmpty(settings.K, settings.D);

            //Adding some more time (TimeForConsensus), for the consensus to be done
            while (DateTimeOffset.Now < settings.ProcessExpiration.Add(settings.TimeForConsensus))
            {
                //Log.InfoFormat("PartialId" +  message.PartialId + " loop in {0} s", stopwatch.Elapsed.TotalSeconds);
                stopwatch.Restart();

                //Ping the corresponding queue, and load up to 20 available messages.
                //[patra]: comment magic numbers.
                var updateMessages = QueueStorage.Get<UpdateAvailableMessage>(PartialReduceQueueName + message.PartialId, 20, 5.Minutes(), 3).ToArray();

                //If no prototypes need to be retrieved, we wait a bit before trying to load them again.
                if (!updateMessages.Any())
                {
                    Thread.Sleep(MilliSeconds);
                    continue;
                }

                var updates = LoadUpdates(updateMessages.ToArray(e=>e.GradientBlobName), message);

                for (int i = 0; i < updates.Length;i++ )
                {
                    var update = updates[i].Prototypes;
                    var workerId = updateMessages[i].WorkerId;

                    //we update local version prototypes with the computed descent term
                    for (int k=0;k < settings.K; k++)
                    {
                        for (int d=0; d< settings.D;d++)
                        {
                            sumOfUpdates.Prototypes[k][d] += update[k][d];
                        }
                        sumOfUpdates.Affectations[k] += updates[i].Affectations[k];
                    }

                    if (!downloadHistory.ContainsKey(workerId))
                    {
                        downloadHistory.Add(workerId, 0);
                    }
                    downloadHistory[workerId]++;
                }

                try
                {
                    stopwatch2.Restart();
                    //Push it into last reduce step

                    var newBlobname = TemporaryBlobName<WPrototypes>.GetNew(settings.Expiration);
                    BlobStorage.PutBlob(newBlobname, sumOfUpdates);

                    //Log.InfoFormat("partialId " + message.PartialId + "pushed new version using " + dictionary.Count + " versions in {0} s", stopwatch2.Elapsed.TotalSeconds);

                    //Push a message in the corresponding queue
                    var updateMessage = 
                        new UpdateAvailableMessage(newBlobname, FinalReduceService.FinaleReduceGroup, message.PartialId);

                    QueueStorage.Put(FinalReduceService.FinalReduceQueueName, updateMessage);

                    QueueStorage.DeleteRange(updateMessages);
                    
                    sumOfUpdates.Empty();
                }
                catch (Exception)
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

        /// <summary>Load the prototypes versions that have been updated (according to the queue).
        ///Loading is run in parallel using as many threads as blobs to load
        ///Do not download several time the same blob in case of multiple message for a same blob
        /// </summary>
        public WPrototypes[] LoadUpdates(TemporaryBlobName<WPrototypes>[] names, PartialReducingMessage message)
        {
            var versions = names.SelectInParallel(e =>
            {
                try
                {
                    return BlobStorage.GetBlob(e);
                }
                catch (Exception)
                {
                    Log.InfoFormat("Exception raised while pulling update in partial reducer " + message.PartialId);
                    return Maybe<WPrototypes>.Empty;
                }

            }, names.Length).Where(w => w.HasValue).ToArray(p => p.Value);

            return versions;
        }
    }
}
