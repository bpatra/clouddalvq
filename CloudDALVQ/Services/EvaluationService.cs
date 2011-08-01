#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CloudDALVQ.BlobNames;
using CloudDALVQ.Common;
using CloudDALVQ.DataGenerator;
using CloudDALVQ.Entities;
using CloudDALVQ;
using CloudDALVQ.Messages;
using Lokad;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Storage;
using Lokad.Cloud.Storage.Shared.Logging;

namespace CloudDALVQ.Services
{
    [QueueServiceSettings(AutoStart = true, 
       Description = "Evaluation service")]
    public class EvaluationService : BaseService<EvaluationMessage>
    {
        protected override void Start(EvaluationMessage message)
        {
            if (message.IsList)
            {
                List();
            }
            else
            {
                Map(message);
            }    
        }

        void List()
        {
            var settings = BlobStorage.GetBlob(SettingsName.Default).Value;
            var snapshotsNames = BlobStorage.ListBlobNames(SnapshotName.GetPrefix(settings.Expiration)).ToArray();

            var counterEntity = BuildBitTreeEntity(snapshotsNames.Length, settings.Expiration);
            int count = 0;
            foreach (var snapshotsName in snapshotsNames)
            {
                Put(new EvaluationMessage
                {
                    IsList = false, 
                    PrototypesName = snapshotsName,
                    CounterEntity = counterEntity,
                    Id = count,
                    SnapshotVersion = snapshotsName.SnapshotVersion
                });
                count++;
            }
        }

        void Map(EvaluationMessage message)
        {
            var settings = BlobStorage.GetBlob(SettingsName.Default).Value;

            var prototypes = BlobStorage.GetBlob(message.PrototypesName).Value;

            if (prototypes.Prototypes.Any(e => e.Any(a => double.IsNaN(a))))
            {
                Log.InfoFormat("some Nan values in the prototypes after " + +prototypes.Affectations.Sum()
                    + " data points processed in " + message.SnapshotVersion +  "Id" + message.Id);
            }
            if (prototypes.Affectations.Any(e => double.IsNaN(e)))
            {
                Log.InfoFormat("some Nan values in the affectations after " + +prototypes.Affectations.Sum()
                    + " data points processed in " + message.SnapshotVersion + "Id" + message.Id);
            }
  
            //HACK : [durut] seed should be changed more wisely
            var generator = DataGeneratorFactory.GetGenerator(settings,message.GetHashCode());
             
            var watch = Stopwatch.StartNew();

            var evaluator = new QuantizationEvaluator(prototypes.Prototypes, generator);
            evaluator.EvaluateWith(settings.EvaluationSampleCount);
            var quantizationError = evaluator.QuantizationError;
            watch.Stop();

            var blobCounter = new BitTreeCounter(message.CounterEntity);
           
            var evaluation = new Evaluation
                                 {
                                     EvaluationDuration = watch.Elapsed.TotalSeconds,
                                     ObservationDate = message.PrototypesName.ObservationDate,
                                     QuantizationError = quantizationError,
                                     SampleCount = settings.EvaluationSampleCount,
                                     Variance = evaluator.Variance,
                                     Affectations = prototypes.Affectations
                                 };
            var evaluationName = new EvaluationName(settings.Expiration, Guid.NewGuid().ToString(),
                                                    message.SnapshotVersion);
            BlobStorage.PutBlob( evaluationName, evaluation);

            var isZero = blobCounter.DecrementAtIndex(message.Id, BlobStorage);
            if(isZero)
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
