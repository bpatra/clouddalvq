#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Runtime.Serialization;
using CloudDALVQ.Entities;
using Lokad.Cloud.Storage;

namespace CloudDALVQ.Messages
{
    [DataContract]
    public class UpdateAvailableMessage
    {
        [DataMember]
        public string PartialId { get; set; }

        [DataMember]
        public string WorkerId { get; set; }

        [DataMember]
        public TemporaryBlobName<WPrototypes> GradientBlobName { get; set; }

        public UpdateAvailableMessage(TemporaryBlobName<WPrototypes> gradientBlobName, string partialId, string workerId)
        {
            GradientBlobName = gradientBlobName;
            PartialId = partialId;
            WorkerId = workerId;
        }
    }
}
