#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Runtime.Serialization;
using CloudDALVQ.BlobNames;
using CloudDALVQ.Common;
using CloudDALVQ.Entities;
using CloudDALVQ.Handy;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Storage;

namespace CloudDALVQ.Services
{
     [QueueServiceSettings(AutoStart = true,
        Description = "Experiment set up service for Batch KMeans")]
    public class BatchSetUpService : BaseService<BatchSetupMessage>
    {
         protected override void Start(BatchSetupMessage message)
         {
             //Pushing settings into the storage. Each worker can retrieve it (alternative to messages).
             var settings = message.Settings;
             BlobStorage.PutBlob(SettingsName.Default, settings);

             // Shared version is initialized. 2006 WC finals as seed.
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
                     Put(new BatchProcessMessage(partialId, workerId, jobId));
                     jobId++;
                 }

                 //Partial reducer messages
                 if (settings.AveragingWith2Layers)
                 {
                     Put(new BatchPartialReducingMessage(i.ToString()));
                 }
             }

             //last reducer message
             Put(new BatchFinalReducingMessage());

             //snapshot service messages
             Put(new BatchSnapshotMessage());
         }
    }

     [DataContract]
     public class BatchSetupMessage
     {
         /// <summary>Learning rate parameter in the gradient descent</summary>
         [DataMember]
         public Settings Settings { get; set; }

         public BatchSetupMessage(Settings settings)
         {
             Settings = settings;
         }
     }
}
