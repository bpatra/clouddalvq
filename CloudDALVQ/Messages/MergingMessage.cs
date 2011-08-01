#region This code is released under the terms of the new BSD licence.
//Authors : Fabrice Rossi, Matthieu Durut
//this projects build a clustering running on Microsoft.Azure.
//the code is build on top of the open source library Lokad.Cloud
//More information at : http://lokadcloud.codeplex.com/ or http://code.google.com/p/lokad-cloud/
#endregion

using System;
using System.Runtime.Serialization;

namespace AsynchronousQuantization
{
    [Serializable, DataContract]
    public class MergingMessage
    {
        /// <summary></summary>
        [DataMember]
        public PrototypesVersionName VersionName{ get; set;}

        public MergingMessage(PrototypesVersionName name)
        {
            VersionName = name;
        }
    }
}
