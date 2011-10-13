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
    /// <summary>
    /// Template that has no particular purpose. It was used for debugging and several other tests.
    /// </summary>
    public class HackTemplate
    {
        private const int M = 1; 
        const int N = 5000;
        const int D = 10;
        const int K = 100;
        const int G = 300;
        const int KnotCount = 305;
        const int BatchSize = 10;
        const int EvaluationCount = 1000;
        private const int IterationBatchKMeans = 5;

        const int PushPeriods = 2;

        private const int MinutesExpiration = 5;

        public static Settings Create()
        {
            return new Settings(N, D, M, K, G, KnotCount, IterationBatchKMeans, BatchSize, PushPeriods, EvaluationCount, 
                                new TimeSpan(0,MinutesExpiration,0),
                                new TimeSpan(0, 0, MinutesExpiration), GeneratorType.UniformInHyperCube, 13, true, false, true, true);
        }
    }
}
