#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CloudDALVQ.Entities;

namespace LocalProcessService
{
    public class ParallelDisplacementExecution
    {
        private const int MaxIterationCount = 4000;
        private const string BasePath = @"../../../Output/CorrectedParallel/";
        private const int Frequency = 100;

        public void Start(Settings settings)
        {
            var writer = File.CreateText(BasePath + "M=" + settings.M + "tau=" + settings.PushPeriods + ".txt");

            var data = ParallelHelpers.GetData(settings);
            var processors = ParallelHelpers.GetDisplacementProcessors(settings);
            var wPrototypes = ParallelHelpers.Initialization(settings);

            var sharedVersion = wPrototypes[0].Clone();
            var sumGradients = ParallelHelpers.Reset(settings);

            for (int t = 0; t < MaxIterationCount; t++)
            {
                var currentIndex = t % settings.N;

                //For each emulated machine, process one point
                for (int m = 0; m < settings.M; m++)
                {
                    processors[m].ProcessSample(data[m][currentIndex], ref wPrototypes[m], ref sumGradients[m]);
                }

                //If we need to merge
                if (t % settings.PushPeriods == 0)
                {
                    for (int p = 0; p < settings.M; p++)
                    {
                        for (int k = 0; k < settings.K; k++)
                        {
                            for (int d = 0; d < settings.D; d++)
                            {
                                sharedVersion.Prototypes[k][d] += sumGradients[p].Prototypes[k][d];
                            }
                        }
                    }
                    for (int p = 0; p < settings.M; p++)
                    {
                        wPrototypes[p] = sharedVersion.Clone();
                    }

                    sumGradients = ParallelHelpers.Reset(settings);
                }

                if (t % Frequency == 0)
                {
                    var error = ParallelHelpers.Evaluate(sharedVersion, settings);
                    writer.WriteLine(t + ";" + error);
                }
            }

            writer.Close();
        }
    }
}

