#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudDALVQ;
using CloudDALVQ.Common;
using CloudDALVQ.Entities;

namespace LocalProcessService
{
    public class MultiGradientProcessor
    {
        public Processor3[] Processors { get; set; }
        public SamplingScheduler[] Schedulers { get; set; }
        public double[][][] Data { get; set; }
        private readonly double[][][] _miniBatch;

        private int P;

        public MultiGradientProcessor(Settings settings)
        {
            Processors = Enumerable.Range(0, settings.M).Select(p => new Processor3()).ToArray();
            Schedulers = Enumerable.Range(0, settings.M).Select(p => new SamplingScheduler(0)).ToArray();
            _miniBatch = Enumerable.Range(0, settings.M).Select(p => new double[settings.BatchSize][]).ToArray();
            P = settings.M;
        }

        public void ProcessMiniBatch(ref WPrototypes[] localProtos, ref WPrototypes[] sumGradients)
        {
            for (int p = 0; p < P; p++)
            {
                Schedulers[p].MakeBatch(Data[p], ref _miniBatch[p]);
                Processors[p].ProcessMiniBatch(_miniBatch[p], ref localProtos[p], ref sumGradients[p], 1);
            }
        }

    }
}

