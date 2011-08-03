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
        const int p = 1;
        const int n = 1000;
        const int d = 100;
        const int k = 100;
        const int g = 150;

        private const int iterationIfBatch = 5;
        const int knotCount = 100;
        const int batchSize = 1;
        const int evaluationCount = 2500;
        const int pushPeriods = 100;
        const double mergingRatio = 0.5;

        private const int seed = 0;
        private const int minutesExpiration = 10;

        private const bool averagingWith2Layers = true;

        private const bool sameInit = true;
        private const bool processingEnabled = true;
        private const bool evaluationEnabled = true;

        public static Settings Create()
        {
            return new Settings(n, d, p, k, g, knotCount, iterationIfBatch, batchSize, pushPeriods,
                evaluationCount, mergingRatio, new TimeSpan(0, minutesExpiration, 0), 
                new TimeSpan(0, 0, 0, 20), GeneratorType.OrthoSplines, seed, averagingWith2Layers,sameInit, processingEnabled, evaluationEnabled);
        }
    }
}
