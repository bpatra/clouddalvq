#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudDALVQ.Entities;

namespace CloudDALVQ.Common
{
    public class Processor2
    {
        /// <summary> </summary>
        private int _stepCount;

        public Processor2()
        {
            _stepCount = 1;
        }

        public void ProcessMiniBatch(double[][] samples, ref WPrototypes localProtos, ref WPrototypes sumGradients, double scaling)
        {
            var N = samples.Length;
            var K = localProtos.Prototypes.Length;
            var D = samples[0].Length;

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
                //double eps = scaling / Math.Sqrt(_stepCount);
                double eps = 0.1;

                for (int d = 0; d < D;d++ )
                {
                    var delta = eps* (localProtos.Prototypes[bestIndex][d] - point[d]);
                    localProtos.Prototypes[bestIndex][d] -= delta;
                    sumGradients.Prototypes[bestIndex][d] -= delta;
                }

                localProtos.Affectations[bestIndex]++;
                sumGradients.Affectations[bestIndex]++;

                _stepCount++;
            }
        }

    }
}
