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
    public class BasicSplines
    {
        private readonly double[] _knots;
        private const int Degree = 3;

        /// <summary>
        ///Manual specification of the knots (and interval) of the vector basis Bsplines.
        /// </summary>
        /// <param name="degree">polynomial degree</param>
        /// <param name="j"> between 0 and knotsCount-n-2</param>
        public BasicSplines(double[] knots)
        {
            _knots = knots;
        }


        public double[][] Evaluation(double[] tt)
        {
            int m = _knots.Length;
            var results = new double[m][];

            for (int j = 0; j <= m - 2; j++)
            {
                var result = new double[tt.Length];
                for (int i = 0; i < tt.Length; i++)
                {
                    result[i] = (_knots[j] <= tt[i] && _knots[j + 1] > tt[i]) ? 1.0 : 0;
                }
                results[j] = result;
            }

            for (int n = 1; n <= Degree; n++)
            {
                for (int j = 0; j <= m - n - 2; j++)
                {
                    var newResult = new double[tt.Length];
                    for (int i = 0; i < tt.Length; i++)
                    {
                        var w1 = (tt[i] - _knots[j])/(_knots[j + n] - _knots[j]);
                        var w2 = (_knots[j + n + 1] - tt[i])/(_knots[j + n + 1] - _knots[j + 1]);
                        newResult[i] = w1*results[j][i] + w2*results[j + 1][i];
                    }
                    results[j] = newResult;
                }
            }


            return results.Take(_knots.Length - Degree - 1).ToArray();
          
        }
    }
}
