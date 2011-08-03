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
using CloudDALVQ.Handy;
using Lokad.Cloud.Storage.Shared.Threading;

namespace CloudDALVQ.Services
{
    [QueueServiceSettings(AutoStart = true,
       Description = "Service that gathers versions owned by PartialReducingServices and produces the averaged version (shared version).")]
    public class FinalReduceService : QueueService<FinalReducingMessage>
    {
        public const string FinalReduceQueueName = "finalreducegradientqueue";
        public const string FinaleReduceGroup = "finalereduceGroup";
        private const int milliSeconds = 100; //[patra]: comment purpose here !

        protected override void Start(FinalReducingMessage message)
        {
            var settings = BlobStorage.GetBlob(SettingsName.Default).Value;

            int count = 1;

            //logging purpose
            var downloadHistory = new Dictionary<string, int>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            //[durut] There is no sense to have different initialisation if we are thinking with gradient instead of prototypes, so we should not use
            //different initialisations anymore.
            var localPrototypes = settings.SameInitialisation ? ProcessService.Initialization(settings, settings.Seed + 19934)
                    : ProcessService.Initialization(settings, settings.Seed + 1);

            //Adding some more time (TimeForConsensus), for the consensus to be done
            while (DateTimeOffset.Now < settings.ProcessExpiration.Add(settings.TimeForConsensus))
            {
                //Log.InfoFormat("PartialId" +  message.PartialId + " loop in {0} s", stopwatch.Elapsed.TotalSeconds);
                stopwatch.Restart();

                //Ping the corresponding queue, and load up to 20 available messages.
                //[patra]: comment magic numbers.
                var updateMessages = QueueStorage.Get<UpdateAvailableMessage>(FinalReduceQueueName, 20, 5.Minutes(), 3).ToArray();

                //If no prototypes need to be retrieved, we wait a bit before trying to load them again.
                if (!updateMessages.Any())
                {
                    Thread.Sleep(milliSeconds);
                    continue;
                }

                var updates = LoadUpdates(updateMessages.ToArray(e => e.GradientBlobName), message);

                for (int i = 0; i < updates.Length; i++)
                {
                    var update = updates[i].Prototypes;
                    var workerId = updateMessages[i].WorkerId;

                    //we update local version prototypes with the computed descent term
                    for (int k = 0; k < settings.K; k++)
                    {
                        for (int d = 0; d < settings.D; d++)
                        {
                            localPrototypes.Prototypes[k][d] += update[k][d];
                        }
                        localPrototypes.Affectations[k] += updates[i].Affectations[k];
                    }

                    if (!downloadHistory.ContainsKey(workerId))
                    {
                        downloadHistory.Add(workerId,0);
                    }
                    downloadHistory[workerId]++;
                }

                try
                {
                    //Push it into last reduce step
                    BlobStorage.PutBlob(new SharedWPrototypesName(settings.Expiration), localPrototypes);
                    count++;
                    //Delete messages from queue
                    QueueStorage.DeleteRange(updateMessages);
                }
                catch (Exception e)
                {
                    Log.InfoFormat("Issue while pushing prototypes in final reducer.");
                }
                
            }

            Log.InfoFormat(count + ": different Shared Version pushed.");

            //Quantization is completed, cleaning the corresponding queue
            QueueStorage.Clear(FinalReduceQueueName);

            if (settings.EvaluationEnabled)
            {
                //Calling evaluation service so snapshots of prototypes could be evaluated
                Put(new EvaluationMessage { IsList = true });
            }
        }

        /// <summary>Load the updates that have been computed (according to the queue).
        ///Loading is run in parallel using as many threads as blobs to load
        /// </summary>
        public WPrototypes[] LoadUpdates(TemporaryBlobName<WPrototypes>[] names, FinalReducingMessage message)
        {
            var versions = names.SelectInParallel(e =>
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

            }, names.Length).Where(w => w.HasValue).ToArray(p => p.Value);

            return versions;
        }
    }
}
