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

    [DataContract]
    public class SharedWPrototypesName2 : BlobName<WPrototypes>
    {
        [Rank(0)]
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        private string _containerName;

        public override string ContainerName
        {
            get { return _containerName; }
        }

        public static SharedWPrototypesName2 InstanceNumber(int i)
        {
            return Containers[i];
        }

        private static readonly SharedWPrototypesName2[] Containers;

        private const int MaxContainers = 20;

        private const int Seed = 28;

        static SharedWPrototypesName2()
        {
            //Random generator is started with the same seed, whatever the worker.
            var random = new Random(Seed);

            //Admissible letters to build container names
            var admissibleLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890".ToCharArray();

            Containers = new SharedWPrototypesName2[MaxContainers];
            for (int i = 0; i < MaxContainers; i++)
            {
                var containerName = new string(Range.Array(12).Select(e => admissibleLetters[random.Next(admissibleLetters.Length)]).ToArray());
                Containers[i] = new SharedWPrototypesName2(containerName, "sharedVersion-" + i);
            }
        }

        private SharedWPrototypesName2(string containerName, string blobName)
        {
            _containerName = containerName;
            Name = blobName;
        }
    }
}

