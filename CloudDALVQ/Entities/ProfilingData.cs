#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CloudDALVQ.Entities
{
    [Serializable, DataContract]
    public class ProfilingData
    {
        [DataMember]
        public string WorkerId { get; set; }

        [DataMember]
        public List<Tuple<string, TimeSpan>> Jobs { get; set; }

        public ProfilingData()
        { }

        public ProfilingData(string jobName, TimeSpan expiration, string workerId)
        {
            Jobs = new List<Tuple<string, TimeSpan>>{ new Tuple<string, TimeSpan>(jobName, expiration) };
            WorkerId = workerId;
        }
    }
}
