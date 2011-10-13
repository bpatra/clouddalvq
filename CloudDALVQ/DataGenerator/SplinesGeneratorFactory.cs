#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudDALVQ;
using CloudDALVQ.Handy;
using Lokad;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Double.Factorization;

namespace CloudDALVQ.DataGenerator
{
    public static class SplinesGeneratorFactory
    {
    
        public static SplinesMixtureGenerator OrthoMixture(int G, int d, int knotCount, int seed)
        {
            const double scale = 100.0;
            double stdDev = 5 *(scale / (double) knotCount);

            int dimension = knotCount - SplinesMixtureGenerator.Degree - 1;
            const int seedCoeff = 2000;
            var unif = new UniformGenerator(dimension, 1.0, seedCoeff);
            double[][] eta = BlockWiseOrthogonal(unif.GetData(G), scale);
            double[] knots = Range.Array(knotCount).ToArray(i => (double)i);
            return new SplinesMixtureGenerator(eta, stdDev, seed, knots, d);
        }



        public static SplinesMixtureGenerator BasicSplineMixture(int G, int d, int knotCount, int seed)
        {
            const double scale = 10.0;
            const double stdDev = 0.5;

            int p = knotCount - SplinesMixtureGenerator.Degree - 2;

            var eta = new double[G][];
            for (int i = 0; i < G; i++)
            {
                var vector = new double[p];
                vector[i%p] = scale;
                eta[i] = vector;
            }

            return new SplinesMixtureGenerator(eta, stdDev, seed, knotCount, d);
        }

        public static double[][] Schmidt(double[][] vectors, double scale)
        {
            int K = vectors.Length;
            int D = vectors[0].Length;
            
            var gram = new double[K][];
            var gramScalar = new double[K];

            for (int k = 0; k < K; k++)
            {
                gram[k] = new double[D];
                Array.Copy(vectors[k], gram[k], D);

                for (int j = 0; j < k; j++)
                {
                    var u = gram[j];
                    var v = vectors[k];

                    double uv = 0;
                    for (int d = 0; d < D; d++)
                    {
                        uv += u[d] * v[d];
                    }

                    double uu = gramScalar[j];
                    double s = (uv/uu);

                    for (int d = 0; d < D;d++ )
                    {
                        gram[k][d] -= s*u[d];
                    }
                }

                for (int d = 0; d < D; d++ )
                {
                    double u = gram[k][d];
                    gramScalar[k] += u*u;
                }  
            }

            for (int k = 0; k < gram.Length; k++)
            {
                var norm = Math.Sqrt(gramScalar[k]);
                var scaleByNorm = scale / norm;
                
                for(int d =0; d < D; d++)
                {
                    gram[k][d] *= scaleByNorm;
                }
            }

            return gram;
        }


        static double[][] BlockWiseOrthogonal(double[][] vectors, double scale)
        {
            int D = vectors[0].Length;
            var results = new List<double[]>();

            var slices = vectors.SliceArray(D);
            foreach (var slice in slices)
            {
                results.AddRange(Schmidt(slice,scale));
            }

            return results.ToArray();
        }
    }
}
