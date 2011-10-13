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
    public class HackTemplate
    {
        private const int p = 1; 
        const int n = 5000;
        const int d = 10;
        const int k = 100;
        const int g = 300;
        const int knotCount = 305;
        const int batchSize = 10;
        const int evaluationCount = 1000;
        private const int iterationIfBatch = 5;

        const int minBatchCountBetween2Writes = 2;

        private const int minutesExpiration = 5;

        public static Settings Create()
        {
            return new Settings(n, d, p, k, g, knotCount, iterationIfBatch, batchSize, minBatchCountBetween2Writes, evaluationCount, 
                                new TimeSpan(0,minutesExpiration,0),
                                new TimeSpan(0, 0, minutesExpiration), GeneratorType.UniformInHyperCube, 13, true, false, true, true);
        }
    }
}
