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
using CloudDALVQ.DataGenerator;
using CloudDALVQ.Entities;

namespace LocalProcessService
{
    public class GradientParallelExecution
    {
        private const int MaxBatchCount = 4000;
        private const string BasePath = @"../../../Output/CorrectedParallel/";
        private const int Frequency = 100;

        public void Start(Settings settings)
        {
            var writer = File.CreateText(BasePath + "M=" + settings.P + "tau=" + settings.PushPeriods + ".txt");

            var data = ParallelHelpers.GetData(settings);
            var multiProcessor = new MultiGradientProcessor(settings) { Data = data };
            var wPrototypes = ParallelHelpers.Initialization(settings);

            var sharedVersion = wPrototypes[0].Clone();
            var sumGradients = ParallelHelpers.Reset(settings);

            int batchcount = 0;
            while (batchcount < MaxBatchCount)
            {
                multiProcessor.ProcessMiniBatch(ref wPrototypes, ref sumGradients);

                if (batchcount % settings.PushPeriods == 0)
                {
                    for (int p = 0; p < settings.P; p++)
                    {
                        for (int k = 0; k < settings.K; k++)
                        {
                            for (int d = 0; d < settings.D; d++)
                            {
                                sharedVersion.Prototypes[k][d] += sumGradients[p].Prototypes[k][d];
                            }
                        }
                    }
                    for (int p= 0;  p <settings.P; p++)
                    {
                        wPrototypes[p] = sharedVersion.Clone();
                    }

                    sumGradients = ParallelHelpers.Reset(settings);
                }

                if (batchcount%Frequency == 0)
                {
                    var error = ParallelHelpers.Evaluate(sharedVersion, settings);
                    writer.WriteLine(batchcount + ";" + error);
                    Console.WriteLine(batchcount);
                }
                batchcount++;
            }
            writer.Close();
        }

    }
}
