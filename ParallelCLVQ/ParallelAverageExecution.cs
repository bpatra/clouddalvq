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
using CloudDALVQ.DataGenerator;
using CloudDALVQ;
using CloudDALVQ.Entities;

namespace LocalProcessService
{
    public class ParallelAverageExecution
    {
        private const int MaxBatchCount = 4000;
        private const string BasePath = @"../../../Output/Parallel/";
        private const int Frequency = 100;

        public void Start(Settings settings)
        {
            var writer = File.CreateText(BasePath + "M=" + settings.M + "tau=" + settings.PushPeriods + ".txt");

            var data = ParallelHelpers.GetData(settings);
            var multiProcessor = new MultiAveragingProcessor(settings) {Data = data};
            var wPrototypes = ParallelHelpers.Initialization(settings);

            int batchcount = 0;
            while (batchcount < MaxBatchCount)
            {
                wPrototypes = multiProcessor.Process(batchcount, wPrototypes);

                if (batchcount%settings.PushPeriods == 0)
                {
                    var sharedProtos = ParallelHelpers.BasicAveraging(wPrototypes);
                    for (int p = 0; p < settings.M; p++)
                    {
                        wPrototypes[p] = sharedProtos.Clone();
                    }
                    if (batchcount%Frequency == 0)
                    {
                        var error = ParallelHelpers.Evaluate(sharedProtos, settings);
                        writer.WriteLine(batchcount + ";" + error);
                    }
                }
                Console.WriteLine(batchcount);
                batchcount++;
            }
            writer.Close();
        }

    }


}
