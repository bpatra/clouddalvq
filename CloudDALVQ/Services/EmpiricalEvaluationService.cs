#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CloudDALVQ.Messages;
using CloudDALVQ;
using CloudDALVQ.BlobNames;
using CloudDALVQ.Common;
using CloudDALVQ.DataGenerator;
using CloudDALVQ.Entities;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Storage;
using Lokad.Cloud.Storage.Shared.Logging;

namespace CloudDALVQ.Services
{
     [QueueServiceSettings(AutoStart = true, 
       Description = "Evaluation service")]
    public class EmpiricalEvaluationService : BaseService<EvaluationMessage>
    {
         protected override void Start(EvaluationMessage message)
         {
             var settings = BlobStorage.GetBlob(SettingsName.Default).Value;
             if (message.IsList)
             {
                 List(settings);
             }
             else
             {
                 Map(message, settings);
             }
         }
         void List(Settings settings)
         {
             
             var snapshotsNames = BlobStorage.ListBlobNames(SnapshotName.GetPrefix(settings.Expiration)).ToArray();
             var counterEntity = BuildBitTreeEntity(snapshotsNames.Length, settings.Expiration);

             int messageId = 0;
             for (int i = 0; i < snapshotsNames.Length; i++)
             {
                 Put(new EvaluationMessage
                         {IsList = false, Id = messageId, CounterEntity = counterEntity, PrototypesName = snapshotsNames[i], SnapshotVersion = snapshotsNames[i].SnapshotVersion});
                 messageId++;
             }
         }

         void Map(EvaluationMessage message, Settings settings)
         {
             var prototypes = BlobStorage.GetBlob(message.PrototypesName).Value;

             var watch = Stopwatch.StartNew();
             double quantizationError = 0;
             for (int i = 0; i < settings.P;i++)
             {
                 var dataGenerator = DataGeneratorFactory.GetGenerator(settings, settings.Seed + i);
                 var data = dataGenerator.GetData(settings.N);
                 for(int d=0; d < data.Length; d++)
                 {
                     double minDist;
                     Util.NearestPrototype(data[d], prototypes.Prototypes, out minDist);
                     quantizationError += minDist;
                 }
             }
             quantizationError /= (settings.N*settings.P);
          
             var blobCounter = new BitTreeCounter(message.CounterEntity);

             var evaluation = new Evaluation
             {
                 EvaluationDuration = watch.Elapsed.TotalSeconds,
                 ObservationDate = message.PrototypesName.ObservationDate,
                 QuantizationError = quantizationError,
                 SampleCount = settings.P* settings.N,
                 Variance = 0,
                 Affectations = prototypes.Affectations
             };
             BlobStorage.PutBlob(new EvaluationName(settings.Expiration, Guid.NewGuid().ToString(), message.SnapshotVersion), evaluation);

             var isZero = blobCounter.DecrementAtIndex(message.Id, BlobStorage);
             if (isZero)
             {
                var allBlobNames = BlobStorage.ListBlobNames(EvaluationName.GetPrefix(settings.Expiration));

                Log.InfoFormat("Starting to write blob with structured evaluation");
                var snapshotWriter = new StringWriter();

                var sharedVersions =
                    allBlobNames.Where(r => r.EvaluationType == SnapshotService.SharedVersion)
                    .Select(b => BlobStorage.GetBlob(b))
                    .Where(mb => mb.HasValue).Select(b => b.Value);
                
                Evaluation.FinalEvaluation(sharedVersions, snapshotWriter);
                var str1 = snapshotWriter.ToString();
                BlobStorage.PutBlob(SettingsName.Container, "LOGSSharedVersion" + settings.StartTime.ToString("dd-HH-mm-ss") + settings.Log, str1);

                var localWriter = new StringWriter();
                var localVersions =
                    allBlobNames.Where(r => r.EvaluationType == SnapshotService.LocalVersion)
                    .Select(b => BlobStorage.GetBlob(b))
                    .Where(mb => mb.HasValue).Select(b => b.Value);
              
                Evaluation.FinalEvaluation(localVersions, localWriter);
                var str2 = localWriter.ToString();
                BlobStorage.PutBlob(SettingsName.Container, "LOGSLocalVersion"+ settings.StartTime.ToString("dd-HH-mm-ss") + settings.Log, str2);
                 Log.InfoFormat("Job complete.");
            
             }
         }

         public static BitTreeEntity BuildBitTreeEntity(int maxValue, DateTimeOffset expiration)
         {
             var containerName = TemporaryBlobName<double>.DefaultContainerName;
             var prefix = TemporaryBlobName<double>.GetNew(expiration).ToString();
             return new BitTreeEntity(maxValue, containerName, prefix);
         }
    }
}
