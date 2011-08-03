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
using Lokad.Cloud.ServiceFabric.Runtime;

namespace LocalProcessService
{
    public class MultiProcessor
    {
        public PrototypeProcessor[] PrototypeProcessors { get; set; }
        public SamplingScheduler[] Schedulers { get; set; }
        public double[][][] Data { get; set; }
        private readonly double[][][] _miniBatch;

        private int P;

        public MultiProcessor(Settings settings)
        {
            PrototypeProcessors = Enumerable.Range(0, settings.M).Select(p => new PrototypeProcessor()).ToArray();
            Schedulers = Enumerable.Range(0, settings.M).Select(p => new SamplingScheduler(0)).ToArray();
            _miniBatch = Enumerable.Range(0, settings.M).Select(p => new double[settings.BatchSize][]).ToArray();
            P = settings.M;
        }

        public WPrototypes[] Process(int batchCount, WPrototypes[] prototypes)
        {
            for (int p = 0; p < P; p++)
            {
                Schedulers[p].MakeBatch(Data[p], ref _miniBatch[p]);
                PrototypeProcessors[p].ProcessMiniBatch(_miniBatch[p], ref prototypes[p], batchCount, 1.0);
            }
            return prototypes;
        }
    }
}

   
