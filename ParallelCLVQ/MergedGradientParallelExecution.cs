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
    /// <summary>
    /// Parallelization using Gradient Parallel scheme but where local version is merged with shared version.
    /// </summary>
    public class MergedGradientParallelExecution
    {
        private const int MaxBatchCount = 4000;
        private const string BasePath = @"../../../Output/MergedCorrectedParallel/";
        private const int Frequency = 100;

        public void Start(Settings settings)
        {
            var writer = File.CreateText(BasePath + "M=" + settings.M + "tau=" + settings.PushPeriods + ".txt");

            var data = ParallelHelpers.GetData(settings);
            var multiProcessor = new MultiGradientProcessor(settings) { Data = data };
            var wPrototypes = ParallelHelpers.Initialization(settings);

            var sharedVersion = wPrototypes[0].Clone();
            var sumGradients = Reset(settings);

            int batchcount = 0;
            while (batchcount < MaxBatchCount)
            {
                multiProcessor.ProcessMiniBatch(ref wPrototypes, ref sumGradients);

                if (batchcount % settings.PushPeriods == 0)
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
                        wPrototypes[p] = Merge(sharedVersion, wPrototypes[p],0.5);
                    }

                    sumGradients = Reset(settings);
                }

                if (batchcount % Frequency == 0)
                {
                    var error = ParallelHelpers.Evaluate(sharedVersion, settings);
                    writer.WriteLine(batchcount + ";" + error);
                    Console.WriteLine(batchcount);
                }
                batchcount++;
            }
            writer.Close();
        }

        static WPrototypes Merge(WPrototypes sharedVersion, WPrototypes localVersion, double ratio)
        {
            int K = localVersion.Prototypes.Length;
            int D = localVersion.Prototypes[0].Length;

            var protos = new double[K][];
            for (int k = 0; k < K; k++)
            {
                var localProtos1 = localVersion.Prototypes[k];
                var sharedProtos = sharedVersion.Prototypes[k];
                var proto = new double[D];
                for (int d = 0; d < D; d++)
                {
                    proto[d] = ratio * localProtos1[d] + (1 - ratio) * sharedProtos[d];
                }
                protos[k] = proto;
            }
            return new WPrototypes{Prototypes = protos, Affectations = new int[K]};
        }

        static WPrototypes[] Reset(Settings settings)
        {
            var gradients = new WPrototypes[settings.M];
            for (int p = 0; p < settings.M; p++)
            {
                var protos = new double[settings.K][];
                for (int k = 0; k < settings.K; k++)
                {
                    var vector = new double[settings.D];
                    protos[k] = vector;
                }
                gradients[p] = new WPrototypes { Prototypes = protos, Affectations = new int[settings.K] };
            }
            return gradients;
        }
    }
}
