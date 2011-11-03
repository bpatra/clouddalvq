#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Linq;
using CloudDALVQ.Entities;

namespace CloudDALVQ.Common
{
    /// <summary>
    /// Deprecated processor, kept only for reproducing experiments proposed in academic publications.
    /// </summary>
    public class AveragingProcessor
    {
        /// <summary> </summary>
        private int _stepCount;

        public AveragingProcessor()
        {
            _stepCount = 1;
        }

        public void ProcessMiniBatch(double[][] samples, ref WPrototypes localProtos, int batchCount, double scaling)
        {
            var N = samples.Length;
            var K = localProtos.Prototypes.Length;
            var D = samples[0].Length;

            var gradient = new double[K][];
            var affs = new int[K];//local minibatch affectations.

            //initialize gradient.
            for (int k = 0; k < K; k++)
            {
                gradient[k] = new double[D];
            }

            //Unroll the loop to avoid latency cost of CPU
            for (int n = 0; n < N; n++)
            {
                var point = samples[n];
                var minDist = double.MaxValue;
                int bestIndex = -1;

                //Finding the nearest prototype
                for (int k = 0; k < K; k++)
                {
                    var centroid = localProtos.Prototypes[k];
                    var dist = 0.0;
                    for (int d = 0; d < D; d++)
                    {
                        var s = point[d] - centroid[d];
                        dist += s * s;
                    }
                    if (dist < minDist)
                    {
                        minDist = dist;
                        bestIndex = k;
                    }
                }
                affs[bestIndex]++;
                for (int d = 0; d < D; d++)
                {
                    gradient[bestIndex][d] += point[d];
                }
            }

            double eps = (scaling / (double)Math.Max(Math.Sqrt(batchCount), 1));
            for (int k = 0; k < K; k++)
            {
                if (affs[k] == 0)
                {
                    continue;
                }
                for (int d = 0; d < D; d++)
                {
                    var val = localProtos.Prototypes[k][d];
                    localProtos.Prototypes[k][d] = (1 - eps) * val + eps * gradient[k][d] / ((double)affs[k]);
                }
                localProtos.Affectations[k] += affs[k];
            }
            _stepCount += N;

        }

        public void ProcessSample(double[] sample, ref WPrototypes localProtos)
        {
            var K = localProtos.Prototypes.Length;
            var D = sample.Length;

            var point = sample;
            var minDist = double.MaxValue;
            int bestIndex = -1;

            //Finding the nearest prototype
            for (int k = 0; k < K; k++)
            {
                var centroid = localProtos.Prototypes[k];
                var dist = 0.0;
                for (int d = 0; d < D; d++)
                {
                    var s = point[d] - centroid[d];
                    dist += s * s;
                }
                if (dist < minDist)
                {
                    minDist = dist;
                    bestIndex = k;
                }
            }

            double eps = (1.0 / (double)Math.Max(Math.Sqrt(_stepCount), 1));

            for (int d = 0; d < D; d++)
            {
                var val = localProtos.Prototypes[bestIndex][d];
                localProtos.Prototypes[bestIndex][d] = (1 - eps) * val + eps * point[d];
            }
            localProtos.Affectations[bestIndex]++;

            _stepCount++;
        }
    }
}
