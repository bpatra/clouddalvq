#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Linq;
using System.Runtime.Serialization;
using CloudDALVQ.Entities;
using CloudDALVQ.Handy;
using Lokad.Cloud.Storage;

namespace CloudDALVQ.BlobNames
{
    public class SharedWPrototypesName : TemporaryBlobName<WPrototypes>
    {
        private const string TypePrefix = "sharedVersion";
       
        [Rank(0)]
        [DataMember]
        public string Name { get; set; }

        public SharedWPrototypesName(DateTimeOffset expiration) : base(expiration, TypePrefix)
        {
        }
    }
}

