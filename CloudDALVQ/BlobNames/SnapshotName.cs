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
    public class SnapshotName : TemporaryBlobName<WPrototypes>
    {
        private const string TypePrefix = "snpashots";

        [DataMember]
        [Rank(0, true)]
        public string SnapshotVersion { get; set; }

        [DataMember]
        [Rank(1, true)]
        public DateTimeOffset ObservationDate { get; set; }

        public SnapshotName(DateTimeOffset expiration, string snapshotType, DateTimeOffset observationDate)
            : base(expiration, TypePrefix)
        {
            ObservationDate = observationDate;
            SnapshotVersion = snapshotType;
        }

        public SnapshotName(DateTimeOffset expiration)
            : base(expiration, TypePrefix)
        {
        }

        /// <summary>Helper to list all local prototypes from a specific (code version/iteration).</summary>
        public static SnapshotName GetPrefix(DateTimeOffset expiration)
        {
            return new SnapshotName(expiration);
        }

      
    }
}
