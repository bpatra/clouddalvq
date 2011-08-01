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
using Lokad.Cloud.Storage;

namespace CloudDALVQ.BlobNames
{
    [Serializable, DataContract]
    public class EvaluationName : TemporaryBlobName<Evaluation>
    {
        private const string TypePrefix = "evaluation";

        [Rank(0, true)]
        [DataMember]
        public string Id { get; set; }

        [Rank(1, true)]
        [DataMember]
        public string EvaluationType { get; set; }

        public EvaluationName(DateTimeOffset expiration, string id, string evaluationType)
            : base(expiration, TypePrefix)
        {
            Id = id;
            EvaluationType = evaluationType;
        }

        /// <summary>Helper to list all evaluations for a specific run.</summary>
        public static EvaluationName GetPrefix(DateTimeOffset expiration)
        {
            return new EvaluationName(expiration,null, null);
        }

    }
}
