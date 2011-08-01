#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CloudDALVQ;
using CloudDALVQ.Entities;

namespace LocalProcessService
{
   public class DelayedGradientParallelExecution
    {
        private const int MaxBatchCount = 4000;
        private const string BasePath = @"../../../Output/DelayedGradientParallel/";
        private const int Frequency = 100;

        public void Start(Settings settings)
        {
            var writer = File.CreateText(BasePath + "M=" + settings.P + "tau=" + settings.PushPeriods + ".txt");

            var data = ParallelHelpers.GetData(settings);
            var multiProcessor = new MultiGradientProcessor(settings) { Data = data };
            var wPrototypes = ParallelHelpers.Initialization(settings);

            var gradients = ParallelHelpers.Reset(settings);

            int D = settings.D;
            int K = settings.K;

            int batchcount = 0;
            var sharedVersion = wPrototypes[0].Clone();
            while (batchcount < MaxBatchCount)
            {
                multiProcessor.ProcessMiniBatch(ref wPrototypes, ref gradients);
                if (batchcount % settings.PushPeriods == 0)
                {
                    for (int p = 0; p < settings.P; p++ )
                    {
                        for (int k = 0; k < K; k++)
                        {
                            for (int d = 0; d < D; d++)
                            {
                                wPrototypes[p].Prototypes[k][d] = sharedVersion.Prototypes[k][d] +
                                                                  gradients[p].Prototypes[k][d];
                            }
                        }
                    }

                    for (int p = 0; p < settings.P; p++)
                    {
                        for (int k = 0; k < K; k++)
                        {
                            for (int d = 0; d < D; d++)
                            {
                                sharedVersion.Prototypes[k][d] += gradients[p].Prototypes[k][d];
                            }
                        }
                    }
                    gradients = ParallelHelpers.Reset(settings);
                }

                if (batchcount % Frequency == 0)
                {
                    var error = ParallelHelpers.Evaluate(wPrototypes[0], settings);
                    writer.WriteLine(batchcount + ";" + error);
                    Console.WriteLine(batchcount);
                }
                batchcount++;
            }
            writer.Close();
        }
    }
}
