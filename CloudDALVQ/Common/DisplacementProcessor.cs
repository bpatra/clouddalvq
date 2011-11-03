#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using CloudDALVQ.Entities;

namespace CloudDALVQ.Common
{
    /// <summary>
    /// Processor that uses displacement terms instead to an averaging logic.
    /// </summary>
    public class DisplacementProcessor
    {
        /// <summary> </summary>
        private int _stepCount;

        public DisplacementProcessor()
        {
            _stepCount = 1;
        }

        public void ProcessMiniBatch(double[][] data, ref WPrototypes localProtos, ref WPrototypes sumGradients, int batchSize)
        {
            var K = localProtos.Prototypes.Length;
            var D = data[0].Length;

            //Unroll the loop to avoid latency cost of CPU
            for (int n = 0; n < batchSize; n++)
            {
                var index = (_stepCount + n) % data.Length;
                var point = data[index];
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

                //double eps = scaling / Math.Sqrt(_stepCount);
                double eps = 0.02;

                for (int d = 0; d < D; d++)
                {
                    var delta = eps * (localProtos.Prototypes[bestIndex][d] - point[d]);
                    localProtos.Prototypes[bestIndex][d] -= delta;
                    sumGradients.Prototypes[bestIndex][d] -= delta;
                }

                localProtos.Affectations[bestIndex]++;
                sumGradients.Affectations[bestIndex]++;

                _stepCount++;
            }
        }

        public void ProcessSample(double[] sample, ref WPrototypes localProtos, ref WPrototypes sumGradients)
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

            double eps = 1.0 / Math.Sqrt(_stepCount);

            for (int d = 0; d < D; d++)
            {
                var delta = eps * (localProtos.Prototypes[bestIndex][d] - point[d]);
                localProtos.Prototypes[bestIndex][d] -= delta;
                sumGradients.Prototypes[bestIndex][d] -= delta;
            }

            localProtos.Affectations[bestIndex]++;
            sumGradients.Affectations[bestIndex]++;

            _stepCount++;
        }
    }
}
