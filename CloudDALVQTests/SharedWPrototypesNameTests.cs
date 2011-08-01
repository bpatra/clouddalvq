#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System.IO;
using CloudDALVQ;
using CloudDALVQ.BlobNames;
using Lokad.Cloud.Storage;
using NUnit.Framework;

namespace CloudDALVQTests
{
    [TestFixture]
    public class SharedWPrototypesNameTests
    {
        [Test]
        public void SharedWPrototypesNamesAreCorrectlyGenerated()
        {
            var firstName = SharedWPrototypesName2.InstanceNumber(0);
            var alsoFirstName = SharedWPrototypesName2.InstanceNumber(0);

            Assert.AreEqual(firstName.ContainerName, alsoFirstName.ContainerName, "#A01");
            Assert.AreEqual(firstName.Name, alsoFirstName.Name, "#A01");
        }

        [Test]
        public void SharedWPrototypesNamesAreCorrectlySerializedAndDeserialized()
        {
            var firstName = SharedWPrototypesName2.InstanceNumber(0);

            var serializer = new CloudFormatter();

            var stream = new MemoryStream();

            serializer.Serialize(firstName, stream);
            stream.Position = 0;
            var deserializedName = (SharedWPrototypesName2) serializer.Deserialize(stream, typeof (SharedWPrototypesName2));

            Assert.AreEqual(firstName.ContainerName, deserializedName.ContainerName, "#A01");
            Assert.AreEqual(firstName.Name, deserializedName.Name, "#A02");
        }

    }
}
