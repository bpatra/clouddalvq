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
    public class DummyGenerator : IGenerator
    {
        private int _d;
        public DummyGenerator(int d)
        {
            _d = d;
        }

        public double[][] GetData(int dataCount)
        {
            var yy = new double[dataCount][];
            for (int i = 0; i < dataCount; i++)
            {
                yy[i] = Next();
            }
            return yy;
        }

        public double[] Next()
        {
            var y = new double[_d];
            return y;
        }
    }
}
