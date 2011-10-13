#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudDALVQ;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;

namespace CloudDALVQ.DataGenerator
{

    /// <summary>
    /// Generator that creates vector with independant Gaussian components.
    /// </summary>
    public class NormalGenerator : IGenerator
    {
        private readonly Normal _noiseRand;
        private readonly int _d;

        public NormalGenerator(double mean, double stdDev, int dimension, int seed)
        {
            _noiseRand = new Normal(mean,stdDev){ RandomSource = new MersenneTwister(seed)};
            _d = dimension;
        }

        public double[][] GetData(int dataCount)
        {
            var array = new double[dataCount][];
            for (int i = 0; i < dataCount; i++)
            {
                array[i] = Next();
            }
            return array;
        }

        public double[] Next()
        {
            var y = new double[_d];
            for(int i=0; i < _d; i++)
            {
                y[i] = _noiseRand.Sample();
            }

            return y;
        }

    }
}
