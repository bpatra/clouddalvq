﻿#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using CloudDALVQ;
using CloudDALVQ.Entities;

namespace CloudDALVQ.Template
{
     [Serializable, DataContract]
    public static class OrthoMixtureTemplate
    {
        const int M = 1;
        const int N = 50000;
        const int D = 1000;
        const int K = 1000;
        const int G = 1500;
        const int KnotCount = 100;

        const int IterationIfBatch = 5;
        const int BatchSize = 1;
        
        const int EvaluationCount = 25000;
        const int PushPeriods = 100;
        private const double MergingRatio = 0.5;

        private const int seed = 17;
        private const int minutesExpiration = 110;
        private const bool sameInit = true;
        private const bool processingEnabled = true;
        private const bool evaluationEnabled = true;
        private const bool averaging2Layers = false;

        public static Settings Create()
        {
            return new Settings(N, D, M, K, G, KnotCount, IterationIfBatch, BatchSize, PushPeriods,
                EvaluationCount, new TimeSpan(0, minutesExpiration, 0),
                new TimeSpan(0, 0, 0, 20), GeneratorType.OrthoSplines, seed, averaging2Layers, sameInit, processingEnabled, evaluationEnabled );
        }
    }
}
