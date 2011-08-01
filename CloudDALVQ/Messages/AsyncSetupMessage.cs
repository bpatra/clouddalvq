#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Runtime.Serialization;
using CloudDALVQ.Entities;

namespace CloudDALVQ.Messages
{
    [DataContract]
    public class AsyncSetupMessage
    {
        /// <summary>Learning rate parameter in the gradient descent</summary>
        [DataMember]
        public Settings Settings { get; set; }

        public AsyncSetupMessage(Settings settings)
        {
            Settings = settings;
        }
    }
}
