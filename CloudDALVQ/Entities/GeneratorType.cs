#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Runtime.Serialization;
using CloudDALVQ;
using CloudDALVQ.Common;

namespace CloudDALVQ.Entities
{
    [DataContract, Serializable, CustomContract]
    public enum GeneratorType
    {
        [EnumMember]
        UniformInHyperCube,

        [EnumMember]
        BasicSplines,

        [EnumMember]
        SparseSplines,

        [EnumMember]
        OrthoSplines,
    }
}
