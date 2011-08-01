#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudDALVQ;

namespace CloudDALVQ.DataGenerator
{
    public class Spline
    {
        private readonly double[] _tt;
        private double[][] _splineRes;
        private readonly int _dimension;
        private const int Degree = 3;
        public Spline(double[] tt,  double[] knots)
        {
            _tt = tt;
            _dimension = knots.Length - Degree - 1;
            TrainBasicSplines(knots);
        }

        void TrainBasicSplines(double[] knots)
        {
            var splines = new BasicSplines(knots);
            _splineRes = splines.Evaluation(_tt);
        }

        public double[] MakeCombination(double[] coeffs)
        {
            if (coeffs.Length != _dimension)
            {
                throw new ArgumentOutOfRangeException("Error in the dimension of the BSplines vector space.");
            }
            var result = new double[_tt.Length];
            for (int j = 0; j < coeffs.Length; j++)
            {
                var coeff = coeffs[j];
                var localSpline = _splineRes[j];
                for (int k = 0; k < _tt.Length; k++)
                {
                    result[k] += coeff * localSpline[k];
                }
            }

            return result;
        }
    }
}
