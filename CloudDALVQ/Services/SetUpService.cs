#region This code is released under the terms of the new BSD licence.
//Authors : Fabrice Rossi, Matthieu Durut
//this projects build a clustering running on Microsoft.Azure.
//the code is build on top of the open source library Lokad.Cloud
//More information at : http://lokadcloud.codeplex.com/ or http://code.google.com/p/lokad-cloud/
#endregion

using System;
using System.Linq;
using Lokad;
using Lokad.Cloud.ServiceFabric;
using Lokad.Quality;
using Lokad.Cloud.Storage;

namespace AsynchronousQuantization
{
    [QueueServiceSettings(AutoStart = true, BatchSize = 1,
        Description = "Experiment set up service")]
    [NoCodeCoverage, UsedImplicitly]
    public class SetUpService : BaseService<AsyncSetUpMessage>
    {
        protected override void Start(AsyncSetUpMessage message)
        {
            //Generating initial shared prototypes
            var dataGenerator = new UniformProvider(ProcessingService.Seed, message.D, ProcessingService.Edge); //TODO: generators must be different on each worker

            int k = message.K;
            int d = message.D;
            var initialSharedPrototypes = new PrototypesVersion()
            {
                Prototypes = dataGenerator.GetData(message.N),
                Weights = Range.Array(k).ToArray(i => 1)
            }; 

            BlobStorage.PutBlob(PrototypesSharedVersionName.Default, initialSharedPrototypes);

            //Pushing messages.
            var jobIdSlices = Range.Array(message.P).SliceArray((int)Math.Ceiling(Math.Sqrt(message.P)));
            int jobId = 0;
            for (int i = 0; i < jobIdSlices.Length; i++)
            {
                for (int j = 0; j < jobIdSlices[i].Length; j++)
                {
                    Put(new ProcessingMessage(message.Expiration, i.ToString(), jobId.ToString(), message.D, message.K,
                                              message.N, message.LearningRate, message.BatchSize));
                    jobId++;
                }

                //Partial reducer messages
                Put(new PartialMergingMessage(i.ToString(), message.D, message.K, message.Expiration));
            }

            //last reducer message
            Put(new FinalMergingMessage(message.D, message.K, message.Expiration));
        }
    }
}
