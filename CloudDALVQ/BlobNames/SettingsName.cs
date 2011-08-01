#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System.Runtime.Serialization;
using CloudDALVQ.Entities;
using Lokad.Cloud.Storage;

namespace CloudDALVQ.BlobNames
{
    public class SettingsName : BlobName<Settings>
    {
        public const string Container = "persistentcontainer";
        static readonly SettingsName Singleton = new SettingsName
        {
            Name = "settings"
        };

        public override string ContainerName
        {
            get { return Container; }
        }

        [Rank(0)]
        [DataMember]
        public string Name { get; set; }

        private SettingsName() { }

        /// <summary>Singleton.</summary>
        public static SettingsName Default
        {
            get { return Singleton; }
        }
    }
}
