#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Runtime.Serialization;

namespace CloudDALVQ.Messages
{
    [Serializable, DataContract]
    public class ProcessMessage
    {
        /// <summary>Id refering to the partial worker (partial merging).</summary>
        [DataMember]
        public string PartialId { get; set; }

        /// <summary>Id refering to the processing worker.</summary>
        [DataMember]
        public string WorkerId { get; set; }

        /// <summary>Seed used for data generation, algorithm initialisation.</summary>
        [DataMember]
        public int Seed { get; set; }

        public ProcessMessage(string partialId, string workerId, int seed)
        {
            PartialId = partialId;
            WorkerId = workerId;
            Seed = seed;
        }
    }
}
