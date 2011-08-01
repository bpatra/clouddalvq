#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
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
        const int p = 1;
        const int n = 50000;
        const int d = 1000;
        const int k = 1000;
        const int g = 1500;
        const int knotCount = 100;

        const int iterationIfBatch = 5;
        const int batchSize = 1;
        const int evaluationCount = 25000;
        const int pushPeriods = 100;
        private const double mergingRatio = 0.5;

        private const int seed = 17;
        private const int minutesExpiration = 110;
        private const bool sameInit = true;
        private const bool processingEnabled = true;
        private const bool evaluationEnabled = true;
        private const bool averaging2Layers = false;

        public static Settings Create()
        {
            return new Settings(n, d, p, k, g, knotCount, iterationIfBatch, batchSize, pushPeriods,
                evaluationCount, mergingRatio, new TimeSpan(0, minutesExpiration, 0),
                new TimeSpan(0, 0, 0, 20), GeneratorType.OrthoSplines, seed, averaging2Layers, sameInit, processingEnabled, evaluationEnabled );
        }
    }
}
