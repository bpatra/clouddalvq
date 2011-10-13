#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Linq;
using CloudDALVQ.BlobNames;
using CloudDALVQ.DataGenerator;
using CloudDALVQ;
using CloudDALVQ.Handy;
using CloudDALVQ.Messages;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Storage;

namespace CloudDALVQ.Services
{
    /// <summary>
    /// Initialisation service, start up the dalvq algorithm.
    /// </summary>
    [QueueServiceSettings(AutoStart = true, Description = "Experiment set up service")]
    public class AsyncSetupService : QueueService<AsyncSetupMessage>
    {
        protected override void Start(AsyncSetupMessage message)
        {
            //Pushing settings into the storage. Each worker can retrieve them (alternative to put them in messages).
            var settings = message.Settings;
            BlobStorage.PutBlob(SettingsName.Default, settings);

            // Shared version is initialized.
            var sharedPrototypes = settings.SameInitialisation ? ProcessService.Initialization(settings, 19934 + settings.Seed) 
                : ProcessService.Initialization(settings, 2006 + settings.Seed);

            BlobStorage.PutBlob(new SharedWPrototypesName(settings.Expiration), sharedPrototypes);

            //Pushing Processing messages.
            //[durut] such logic is more subtile than it appears do not modify.
            var jobIdSlices = Range.Array(settings.M).SliceArray((int)Math.Ceiling(Math.Sqrt(settings.M)));
            int jobId = 0;
            for (int i = 0; i < jobIdSlices.Length; i++)
            {
                for (int j = 0; j < jobIdSlices[i].Length; j++)
                {
                    string partialId = i.ToString();
                    string workerId = jobId.ToString();
                    Put(new ProcessMessage(partialId, workerId, jobId));
                    jobId++;
                }

                //Partial reducer messages
                if (settings.Reducing2Layers)
                {
                    Put(new PartialReducingMessage(i.ToString()));
                }
            }

            //last reducer message
            Put(new FinalReducingMessage());

            //snapshot service messages
            Put(new SnapshotMessage());
        }
    }
}
