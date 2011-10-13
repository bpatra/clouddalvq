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
            const int p = 1;
            const int n = 1000;
            const int d = 2;
            const int k = 5;
            const int g = 50;
            const int knotCount = 30;
            const int batchSize = 30;
            const int pushPeriods = 5;
            const int evaluationCount = 20000;
            const double mergingRatio = 1.0;
            private const int iterationIfBatch = 5;
            private const bool averagingWith2Layers = true;

            private const int seed = 17;

            public static Settings Create()
            {
                return new
                    Settings(n, d, p, k, g, knotCount, iterationIfBatch, batchSize, pushPeriods, evaluationCount, new TimeSpan(0, 10,0), 
                             new TimeSpan(0,1,0), GeneratorType.UniformInHyperCube, seed, averagingWith2Layers,true, true, true);
            }
    }
}
