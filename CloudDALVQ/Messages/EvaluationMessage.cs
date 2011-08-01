#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Runtime.Serialization;
using CloudDALVQ.BlobNames;
using CloudDALVQ.Common;
using Lokad.Cloud.Storage;

namespace CloudDALVQ.Messages
{
    [Serializable, DataContract]
    public class EvaluationMessage
    {
        [DataMember]
        public bool IsList { get; set; }

        [DataMember]
        public BitTreeEntity CounterEntity { get; set; }

        [DataMember]
        public SnapshotName PrototypesName { get; set; }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string SnapshotVersion { get; set; }
    }
}
