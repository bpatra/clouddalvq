#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Runtime.Serialization;
using CloudDALVQ;

namespace CloudDALVQ.Entities
{
    [Serializable, DataContract]
    public class Settings
    {
        /// <summary>Number of data points generated per<see cref="ProcessService"/> instances.</summary>
        [DataMember]
        public int N { get; set; }

        /// <summary>Data dimension as (double vector).</summary>
        [DataMember]
        public int D { get; set; }

        /// <summary>Number of "Mappers" i.e., <see cref="ProcessService"/> instances.</summary>
        [DataMember]
        public int M { get; set; }

        /// <summary>Number of prototypes.</summary>
        [DataMember]
        public int K { get; set; }

        /// <summary>Number of splines center in the mixture.</summary>
        [DataMember]
        public int G { get; set; }

        /// <summary>Number of knots (more knots => more complex curve ).</summary>
        [DataMember]
        public int KnotCount { get; set; }

        /// <summary>Number of iterations for a batch-KMeans.</summary>
        [DataMember]
        public int IterationBatchKMeans { get; set; }

        /// <summary>Number of points to be processed in evaluation of a version of prototypes.</summary>
        [DataMember]
        public int EvaluationSampleCount { get; set; }

        /// <summary>Minimum number of batchs required for a new push action on the shared version.</summary>
        [DataMember]
        public double PushPeriods { get; set; }

        /// <summary>Number of points needed to be process before a cycle of push thread and pull threads.</summary>
        [DataMember]
        public int MiniGroupSize { get; set; }

        /// <summary>Expiration date time for temporary entities</summary>
        [DataMember]
        public DateTimeOffset Expiration { get;set;}

        ///<summary>Maximum duration for the process loop.</summary>
        [DataMember]
        public DateTimeOffset ProcessExpiration { get; set; }

        ///<summary>Date time at the begining of the execution.</summary>
        [DataMember]
        public DateTimeOffset StartTime { get; set; }

        /// <summary>Number of data points generated per worker.</summary>
        [DataMember]
        public TimeSpan TimeForConsensus { get; set; }

        /// <summary>Type of data generated.</summary>
        [DataMember]
        public GeneratorType GeneratorType { get; set; }

        /// <summary></summary>
        [DataMember]
        public int Seed { get; set; }

        /// <summary>If enabled, prototypes initialisation is the same in each worker and in shared version.</summary>
        [DataMember]
        public bool SameInitialisation { get; set; }

        /// <summary>If disabled, <see cref="ProcessService"/> instances are only communicating.</summary>
        [DataMember]
        public bool ProcessingEnabled { get; set; }

        /// <summary>If disabled, no evaluation.</summary>
        [DataMember]
        public bool EvaluationEnabled { get; set; }

        [DataMember]
        public string Log { get; set; }

        /// <summary>Reducing can be done with 1 layer (small latency) or 2 layers (bigger scale-up).</summary>
        [DataMember]
        public bool Reducing2Layers { get; set; }

        public Settings(int n, int d, int m, int k, int g, int knotCount, int iterationBatchKMeans,
            int batchSize, int pushPeriods, int evaluationCount,  TimeSpan timeForProcessing, 
            TimeSpan timeForConsensus, GeneratorType genType, int seed, bool reducing2Layers,
            bool sameInitialisation, bool processingEnabled, bool evaluationEnabled)
        {
            N = n;
            D = d;
            M = m;
            K = k;
            G = g;
            IterationBatchKMeans = iterationBatchKMeans;
            KnotCount = knotCount;
            MiniGroupSize = batchSize;
            PushPeriods = pushPeriods;
           
            EvaluationSampleCount = evaluationCount;
            TimeForConsensus = timeForConsensus;
            GeneratorType = genType;
            Seed = seed;

            StartTime = DateTimeOffset.UtcNow;
            ProcessExpiration = StartTime.Add(timeForProcessing);
            Expiration = ProcessExpiration.Add(TimeForConsensus).AddHours(1);

            Reducing2Layers = reducing2Layers;

            SameInitialisation = sameInitialisation;
            ProcessingEnabled = processingEnabled;
            EvaluationEnabled = evaluationEnabled;
        }
    }
}
