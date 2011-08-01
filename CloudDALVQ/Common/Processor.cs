#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Linq;
using CloudDALVQ.Entities;

namespace CloudDALVQ.Common
{
    public class Processor
    {
        /// <summary> </summary>
        private int _stepCount;


        public Processor()
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

        /// <summary>
        /// experimenting new ideas on gradient.
        /// </summary>
        public static WPrototypes ApplyDeplacement(WPrototypes sharedVersion)
        {
            int K = sharedVersion.Prototypes.Length;
            int D = sharedVersion.Prototypes[0].Length;

            var protos = new double[K][];
            for (int k = 0; k < K; k++)
            {
                protos[k] = new double[D];
                for (int d = 0; d < D; d++)
                {
                    protos[k][d] = sharedVersion.Prototypes[k][d];//+ localProto[d] - oldLocalProto[d];
                }
            }
            return new WPrototypes{Prototypes = protos, Affectations = new int[K]};
        }

        public void LocalMerge(ref WPrototypes localVersion, WPrototypes sharedVersion, double ratio)
        {
            if (ratio == 0.0)
            {
                localVersion = sharedVersion.Clone();
                return;
            }

            if (ratio == 1.0)//Pure local execution (shared version is not taken into account)
            {
                return;
            }

            int K = localVersion.Prototypes.Length;
            int D = localVersion.Prototypes[0].Length;
            for (int k = 0; k < K; k++)
            {
                var localProtos1 = localVersion.Prototypes[k];
                var sharedProtos = sharedVersion.Prototypes[k];

                for (int d = 0; d < D; d++)
                {
                    localProtos1[d] = ratio * localProtos1[d] + (1 - ratio) * sharedProtos[d];
                }
            }
        }

        public void LocalMerge(ref WPrototypes localVersion, WPrototypes oldLocalVersion, WPrototypes sharedVersion, double ratio)
        {
            if (ratio == 0.0)
            {
                localVersion = sharedVersion.Clone();
                return;
            }

            if (ratio == 1.0)//Pure local execution (shared version is not taken into account)
            {
                return;
            }

            int K = localVersion.Prototypes.Length;
            int D = localVersion.Prototypes[0].Length;
            for (int k = 0; k < K; k++)
            {
                var oldLocalProto = oldLocalVersion.Prototypes[k];
                var localProto = localVersion.Prototypes[k];
                var sharedProtos = sharedVersion.Prototypes[k];

                for (int d = 0; d < D; d++)
                {
                    localProto[d] = ratio * oldLocalProto[d] + (1 - ratio) * sharedProtos[d]
                        + localProto[d] - oldLocalProto[d];
                }
            }
        }
    }
}
