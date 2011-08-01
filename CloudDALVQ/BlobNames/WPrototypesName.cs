#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
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
    public class WPrototypesName : TemporaryBlobName<WPrototypes>
    {
        const string TypePrefix = "prototypesVersion";

        [DataMember]
        [Rank(0)]
        public string PartialId { get; set; }

        [DataMember]
        [ Rank(1)]
        public string WorkerId { get; set; }

        public WPrototypesName(DateTimeOffset expiration, string partialId, string workerId)
            : base(expiration, TypePrefix)
        {
            PartialId = partialId;
            WorkerId = workerId;
        } 
    }
}
