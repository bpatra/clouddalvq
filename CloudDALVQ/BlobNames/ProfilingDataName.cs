#region Copyright (c) 2011--, Benoit PATRA <benoit.patra@gmail.com> and Matthieu Durut <durut.matthieu@gmail.com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Runtime.Serialization;
using CloudDALVQ.Entities;
using Lokad.Cloud.Storage;

namespace CloudDALVQ.BlobNames
{
    [Serializable, DataContract]
    public class ProfilingDataName : TemporaryBlobName<ProfilingData>
    {
        const string TypePrefix = "profiling-data";

        [Rank(0)]
        [DataMember]
        public Guid ProfilingId { get; set; }

        public ProfilingDataName(DateTimeOffset expiration)
            : base(expiration, TypePrefix)
        {
            ProfilingId = Guid.NewGuid();
        }
    }
}
