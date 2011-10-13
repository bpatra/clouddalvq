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
    public class UniformWeirdTemplate
    {
            const int M = 1;
            const int N = 1000;
            const int D = 2;
            const int K = 5;
            const int G = 50;
            const int KnotCount = 30;
            const int BatchSize = 30;
            const int PushPeriods = 5;
            const int EvaluationCount = 20000;
            private const int IterationBatchKMeans = 5;
            private const bool Reducing2Layers = true;

            private const int Seed = 17;

            public static Settings Create()
            {
                return new
                    Settings(N, D, M, K, G, KnotCount, IterationBatchKMeans, BatchSize, PushPeriods, EvaluationCount, new TimeSpan(0, 10,0), 
                             new TimeSpan(0,1,0), GeneratorType.UniformInHyperCube, Seed, Reducing2Layers,true, true, true);
            }
    }
}
