#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.IO;
using CloudDALVQ.Entities;
using CloudDALVQ.Handy;

namespace LocalProcessService
{
    public class ParallelRandomDelayedExecution
    {
        private const int MaxIterationCount = 3000;
        private const string BasePath = @"../../../Output/RandomDelayedParallelDisplacement/";
        private const int Frequency = 100;

        private static readonly Random Generator = new Random(17);

        public void Start(Settings settings)
        {
            var writer = File.CreateText(BasePath + "M=" + settings.M + "tau=" + settings.PushPeriods + ".txt");

            var data = ParallelHelpers.GetData(settings);
            var processors = ParallelHelpers.GetDisplacementProcessors(settings);
            var wPrototypes = ParallelHelpers.Initialization(settings);

            var sharedVersion = wPrototypes[0].Clone();
            var sumGradients = ParallelHelpers.Reset(settings);

            WPrototypes[] incomingSharedVersions = Range.Array(settings.M).ToArray(i => sharedVersion.Clone()); //one incoming shared version per emulated machine
            WPrototypes[] uploadedDisplacements = Range.Array(settings.M).ToArray(i => sumGradients[i]); //one displacement term per emulated machine

            int[] incomingTimes = Range.Array(settings.M).ToArray(i => 1);//NextTimeSpan(Tau, 2*Tau)); //one next communication time per emulated machine

            for (int t = 0; t < MaxIterationCount; t++)
            {
                var currentIndex = t % settings.N;

                //For each emulated machine, process one point
                for (int m = 0; m < settings.M; m++)
                {
                    processors[m].ProcessSample(data[m][currentIndex], ref wPrototypes[m], ref sumGradients[m]);
                }

                //READ
                for (int m = 0; m < settings.M;m++ )
                {
                    //if it's time to read
                    if (incomingTimes[m] == t)
                    {
                        for (int k = 0; k < settings.K; k++)
                        {
                            for (int d = 0; d < settings.D; d++)
                            {
                                wPrototypes[m].Prototypes[k][d] = incomingSharedVersions[m].Prototypes[k][d] +
                                                                  sumGradients[m].Prototypes[k][d];
                            }
                        }

                        incomingSharedVersions[m] = sharedVersion.Clone();
                    }
                }

                //WRITE
                for (int m = 0; m < settings.M;m++ )
                {
                    //if it's time to write
                    if (incomingTimes[m] == t)
                    {
                        for (int k = 0; k < settings.K; k++)
                        {
                            for (int d = 0; d < settings.D; d++)
                            {
                                sharedVersion.Prototypes[k][d] += uploadedDisplacements[m].Prototypes[k][d];
                            }
                        }

                        //Preparing next incoming I/O
                        incomingTimes[m] += NextTimeSpan((int) settings.PushPeriods, (int)(2 * settings.PushPeriods));
                        //incomingSharedVersions[m] = sharedVersion.Clone();
                        uploadedDisplacements[m] = sumGradients[m].Clone();

                        //resetting the displacement term
                        sumGradients[m].Empty();
                    }
                }  

                if (t % Frequency == 0)
                {
                    var error = ParallelHelpers.Evaluate(sharedVersion, settings);
                    writer.WriteLine(t + ";" + error);
                }
            }

            writer.Close();
        }

        static int NextTimeSpan(int min, int max)
        {
            return min + Generator.Next(max - min);
        }
    }
}
