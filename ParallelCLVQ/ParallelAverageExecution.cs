#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System.IO;
using CloudDALVQ.Entities;

namespace LocalProcessService
{
    public class ParallelAverageExecution
    {
        private const int MaxIterationCount = 4000;
        private const int Frequency = 100;
        private const string BasePath = @"../../../Output/Parallel/";

        public void Start(Settings settings)
        {
            var writer = File.CreateText(BasePath + "M=" + settings.M + "tau=" + settings.PushPeriods + ".txt");

            var data = ParallelHelpers.GetData(settings);
            var processors = ParallelHelpers.GetAveragingProcessors(settings);
            var wPrototypes = ParallelHelpers.Initialization(settings);

            for (int t = 0; t < MaxIterationCount; t++)
            {
                var currentIndex = t % settings.N;

                //For each emulated machine, process one point
                for (int m = 0; m < settings.M; m++)
                {
                    processors[m].ProcessSample(data[m][currentIndex], ref wPrototypes[m]);
                }

                //If we need to merge
                if (t % settings.PushPeriods == 0)
                {
                    var sharedProtos = ParallelHelpers.BasicAveraging(wPrototypes);
                    for (int p = 0; p < settings.M; p++)
                    {
                        wPrototypes[p] = sharedProtos.Clone();
                    }

                    if (t % Frequency == 0)
                    {
                        var error = ParallelHelpers.Evaluate(sharedProtos, settings);
                        writer.WriteLine(t + ";" + error);
                    }
                }
            }

            writer.Close();
        }
    }
}
