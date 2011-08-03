#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Runtime.Serialization;
using System.Threading;
using CloudDALVQ.BlobNames;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Storage;

namespace CloudDALVQ.Services
{
    /// <summary>Snapshot service downloads prototypes shared version, and push them into 
    /// a dedicated part of the storage. This is used to evaluate evolution of loss function with time.</summary>
    [QueueServiceSettings(AutoStart = true,
       Description = "Snapshot service gets snapshot of prototypes and store them in a dedicated part of the storage ")]
    public class BatchSnapshotService : QueueService<BatchSnapshotMessage>
    {
        public const int PingFreqSec = 10;// previous3;
        public const string SharedVersion = "sharedType";

        protected override void Start(BatchSnapshotMessage message)
        {
            var settings = BlobStorage.GetBlob(SettingsName.Default).Value;
            var lastUpdate = DateTimeOffset.MinValue;
            var timeSpan = new TimeSpan(0, 0, PingFreqSec);

            int iterationRetrieved = 0;

            //Same condition as in the merging services
            while (iterationRetrieved < settings.IterationIfBatchKMeans)
            {
                //Add conditions to avoid massive stress on blobstorage.
                if (DateTimeOffset.Now > lastUpdate + timeSpan)
                {
                    //Get shared prototypes and push them into the storage.
                    var sharedVersionName = new WPrototypesName(settings.Expiration, "shared", iterationRetrieved.ToString());
                    var sharedProtos = BlobStorage.GetBlob(sharedVersionName);
                    
                    if (sharedProtos.HasValue)
                    {
                        var now = DateTimeOffset.Now;
                        BlobStorage.PutBlob(new SnapshotName(settings.Expiration, SharedVersion, now), sharedProtos.Value);

                        iterationRetrieved++;
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

    [Serializable, DataContract]
    public class BatchSnapshotMessage
    {
        public BatchSnapshotMessage()
        {

        }
    }
}
