#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lokad;

namespace CloudDALVQ.Common
{
    public class SamplingScheduler
    {
        private int _sampleIndex;

        public SamplingScheduler(int sampleIndex)
        {
            _sampleIndex = sampleIndex; 
        }

        public void MakeBatch(double[][] data, ref double[][] batch)
        {
            for (int i = 0; i < batch.Length; i++)
            {
                batch[i] = data[_sampleIndex];
                _sampleIndex = (_sampleIndex + 1) % data.Length;
            }
        }
    }
}
