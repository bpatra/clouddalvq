#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudDALVQ;
using CloudDALVQ.Entities;

namespace CloudDALVQ.Template
{
    public class OrthoMixtureLowScaleTemplate
    {
        const int M = 1;
        const int N = 1000;
        const int D = 100;
        const int K = 100;
        const int G = 150;

        private const int IterationsBatchKMeans = 5;
        const int KnotCount = 100;
        const int BatchSize = 1;
        const int EvaluationCount = 2500;
        const int PushPeriods = 100;

        private const int Seed = 0;
        private const int MinutesExpiration = 10;

        private const bool Reducing2Layers = true;

        private const bool SameInit = true;
        private const bool ProcessingEnabled = true;
        private const bool EvaluationEnabled = true;

        public static Settings Create()
        {
            return new Settings(N, D, M, K, G, KnotCount, IterationsBatchKMeans, BatchSize, PushPeriods,
                EvaluationCount,  new TimeSpan(0, MinutesExpiration, 0), 
                new TimeSpan(0, 0, 0, 20), GeneratorType.OrthoSplines, Seed, Reducing2Layers,SameInit, ProcessingEnabled, EvaluationEnabled);
        }
    }
}
