#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudDALVQ.DataGenerator;
using CloudDALVQ;
using CloudDALVQ.Handy;
using Lokad;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;

namespace CloudDALVQ.DataGenerator
{
    /// <summary>
    /// Generation of noisy mixture of third degree splines.
    /// </summary>
    /// <remarks>Notation from <see url="http://www-bcf.usc.edu/~gareth/research/fclust.pdf" /></remarks>
    public class SplinesMixtureGenerator : IGenerator
    {
        public const int Degree = 3;

        private readonly double _stdDev;
        private readonly double[][] _eta;
        private readonly int _samplingSize;
        private readonly double[][] _centers;
        private readonly double[] _knots;

        private readonly DiscreteUniform _clusterRand;
        private readonly Normal _noiseRand;
        /// <summary>
        /// Create samples
        /// </summary>
        /// <param name="eta">vectors of coefficients for splines.
        ///  The vector is of size (number or mixture component) x (number of Basic splines). G X p (in the articles </param>
        /// <param name="stdDev">Standard deviation for the white noise added on the functions.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="knotCount">Number of knots on the splines.</param>
        /// <param name="samplingSize">Size of the observed vectors.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public SplinesMixtureGenerator(double[][] eta, double stdDev, int seed, int knotCount, int samplingSize)
        {
            if (eta.Length == 0 || eta[0].Length != knotCount - Degree - 2)
            {
                throw new ArgumentOutOfRangeException("Unable to create such B Splines Generator.");
            }
            
            _stdDev = stdDev;
            _eta = eta;
            _knots = Range.Array(knotCount).ToArray(i => (double)i / (knotCount - 1));
            _samplingSize = samplingSize;
            var  rand = new MersenneTwister(seed);
            var rand2 = new MersenneTwister(seed + 1);
            _clusterRand = new DiscreteUniform(0, _eta.Length - 1) { RandomSource = rand };
            _noiseRand = new Normal(0, _stdDev){RandomSource = rand2};
            _centers = GetCenters();
            
        }

        /// <summary>
        /// Create samples
        /// </summary>
        /// <param name="eta">vectors of coefficients for splines.
        ///  The vector is of size (number or mixture component) x (number of Basic splines). G X p (in the articles </param>
        /// <param name="stdDev">Standard deviation for the white noise added on the functions.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="knotCount">Number of knots on the splines.</param>
        /// <param name="samplingSize">Size of the observed vectors.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public SplinesMixtureGenerator(double[][] eta, double stdDev, int seed, double[] knots, int samplingSize)
        {
            if (eta.Length == 0 || eta[0].Length != knots.Length - Degree - 1)
            {
                throw new ArgumentOutOfRangeException("Unable to create such B Splines Generator.");
            }

            _stdDev = stdDev;
            _eta = eta;
            _knots = knots;
            _samplingSize = samplingSize;
            var rand = new MersenneTwister(seed);
            var rand2 = new MersenneTwister(seed + 1);
            _clusterRand = new DiscreteUniform(0, _eta.Length - 1) { RandomSource = rand };
            _noiseRand = new Normal(0, _stdDev) { RandomSource = rand2 };
            _centers = GetCenters();
        }

        public double[][] GetData(int dataCount)
        {
            var values = new double[dataCount][];
            for(int i=0; i < dataCount; i++)
            {
                values[i] = Next();
            }

            return values;
        }

        public double[] Next()
        {
            int k = _clusterRand.Sample();

            var y= new double[_samplingSize];
            for (int t = 0; t < _samplingSize; t++)
            {
                y[t] = _centers[k][t] + _noiseRand.Sample();
            }
            return y;  
        }

        /// <summary>
        /// Returns the Bsplines centers of the mixture. 
        /// </summary>
        /// <returns></returns>
        public double[][] GetCenters()
        {
            var centers = new double[_eta.Length][];
             var tt = Enumerable.Range(0, _samplingSize).ToArray(i => i*_knots[_knots.Length - 1]/(double) _samplingSize);
            var spline = new Spline(tt,  _knots);
           
            for (int k = 0; k < _eta.Length; k++ )
            {
                var coeffs = _eta[k];
                centers[k] = spline.MakeCombination(coeffs);
            }
            return centers;
        }

        public double[] GetXLabel()
        {
            return Range.Array(_samplingSize).ToArray(t => t * _knots[_knots.Length - 1] / (double)_samplingSize);
        }
    }
}
