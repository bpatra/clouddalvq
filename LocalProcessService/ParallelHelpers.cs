#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudDALVQ;
using CloudDALVQ.DataGenerator;
using CloudDALVQ.Entities;

namespace LocalProcessService
{
    public static class ParallelHelpers
    {
        public static double[][][] GetData(Settings settings)
        {
            var data = new double[settings.M][][];
            for (int p = 0; p < settings.M; p++)
            {
                data[p] = (DataGeneratorFactory.GetGenerator(settings, settings.Seed + p))
                .GetData(settings.N);
            }
            return data;
        }

        public static double Evaluate(WPrototypes prototypes, Settings settings)
        {
            var data = GetData(settings);
            double error = 0;
            for (int p = 0; p < settings.M; p++)
            {
                for (int i = 0; i < data[p].Length; i++)
                {
                    double minDist;
                    Util.NearestPrototype(data[p][i], prototypes.Prototypes, out minDist);
                    error += minDist;
                }
            }
            return error / settings.M;
        }

        public static WPrototypes[] Initialization(Settings settings)
        {
            return Enumerable.Range(0, settings.M)
                .Select(p => ProcessService.Initialization(settings, settings.M + 1))
                .ToArray();
        }

        public static WPrototypes BasicAveraging(WPrototypes[] protos)
        {
            int K = protos[0].Prototypes.Length;
            int D = protos[0].Prototypes[0].Length;

            var average = new double[K][];
            for (int k = 0; k < K; k++)
            {
                var avg = new double[D];

                for (int d = 0; d < D; d++)
                {
                    for (int p = 0; p < protos.Length; p++)
                    {
                        avg[d] += (1 / (double)protos.Length) * protos[p].Prototypes[k][d];
                    }
                }
                average[k] = avg;
            }
            return new WPrototypes { Affectations = new int[K], Prototypes = average };
        }

        public static WPrototypes[] Reset(Settings settings)
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
