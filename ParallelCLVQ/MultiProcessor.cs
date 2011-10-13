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
        public AveragingProcessor[] AveragingProcessors { get; set; }
        public Sampler[] Schedulers { get; set; }
        public double[][][] Data { get; set; }
        private readonly double[][][] _miniGroups;

        private readonly int M;

        public MultiProcessor(Settings settings)
        {
            AveragingProcessors = Enumerable.Range(0, settings.M).Select(p => new AveragingProcessor()).ToArray();
            Schedulers = Enumerable.Range(0, settings.M).Select(p => new Sampler(0)).ToArray();
            _miniGroups = Enumerable.Range(0, settings.M).Select(p => new double[settings.MiniGroupSize][]).ToArray();
            M = settings.M;
        }

        public WPrototypes[] Process(int batchCount, WPrototypes[] prototypes)
        {
            for (int p = 0; p < M; p++)
            {
                Schedulers[p].MakeBatch(Data[p], ref _miniGroups[p]);
                AveragingProcessors[p].ProcessMiniBatch(_miniGroups[p], ref prototypes[p], batchCount, 1.0);
            }
            return prototypes;
        }
    }
}

   
