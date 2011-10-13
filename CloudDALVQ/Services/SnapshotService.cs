#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Threading;
using CloudDALVQ.BlobNames;
using CloudDALVQ.Messages;
using CloudDALVQ.Services;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Storage;

namespace CloudDALVQ.Services
{
    /// <summary>Snapshot service downloads shared version and push them into 
    /// a dedicated part of the storage. This is used to evaluate evolution of distortion with time.</summary>
    [QueueServiceSettings(AutoStart = true, 
       Description = "Snapshot service gets snapshot of prototypes and store them in a dedicated part of the storage ")]
    public class SnapshotService : QueueService<SnapshotMessage>
    {
        public const int PingFreqSec = 10;// previous3;
        public const string SharedVersion = "sharedType";
        public const string LocalVersion = "localType";

        protected override void Start(SnapshotMessage message)
        {
            var settings = BlobStorage.GetBlob(SettingsName.Default).Value;
            var lastUpdate = DateTimeOffset.MinValue;
            var timeSpan = new TimeSpan(0, 0, PingFreqSec);

            //Same condition as in the merging services
            while (DateTimeOffset.Now < settings.ProcessExpiration.Add(settings.TimeForConsensus))
            {
                //Add conditions to avoid massive stress on blobstorage.
                if  (DateTimeOffset.Now > lastUpdate + timeSpan)
                {
                    //Get shared prototypes and push them into the storage.
                    var sharedProtos = BlobStorage.GetBlob(new SharedWPrototypesName(settings.Expiration));
                    var now = DateTimeOffset.Now;
                    BlobStorage.PutBlob(new SnapshotName(settings.Expiration, SharedVersion, now), sharedProtos.Value);

                    //Get local prototypes of woker 0
                    var localProtos = BlobStorage.GetBlob(new WPrototypesName(settings.Expiration, "0", "0"));
                    if(localProtos.HasValue)
                    {
                        BlobStorage.PutBlob(new SnapshotName(settings.Expiration, LocalVersion, now), localProtos.Value);
                    }
                    lastUpdate = DateTimeOffset.Now;
                }

                else
                {
                    Thread.Sleep(timeSpan);
                }
            }
        } 
    }
}
