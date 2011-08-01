#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Runtime.Serialization;

namespace CloudDALVQ.Messages
{
    [Serializable, DataContract]
    public class PartialReducingMessage
    {
         /// <summary></summary>
        [DataMember]
        public string PartialId { get; set;}

        public PartialReducingMessage(string partialId)
        {
            PartialId = partialId;
        }
    }
}
