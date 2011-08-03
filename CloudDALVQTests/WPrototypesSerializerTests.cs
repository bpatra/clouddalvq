#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CloudDALVQ.Common;
using CloudDALVQ;
using CloudDALVQ.Entities;
using Lokad.Cloud.Storage.Shared;
using NUnit.Framework;

namespace CloudDALVQTests
{
    [TestFixture]
    public class WPrototypesSerializerTests
    {
        [Test]
        public void SerializeAndDeserializeAreInverse()
        {
            var prototypes = new [] { new[] { 1.0, 2.1, 3.1 }, new[] { 1.3, 1.5, 4.5 } };
            var affs = new[] { 1, 5 };
            var wPrototypes = new WPrototypes() { Prototypes = prototypes, Affectations = affs };
            using (var stream = new MemoryStream())
            {
                var serializer = new NewCustomFormatter() as IDataSerializer;
                serializer.Serialize(wPrototypes, stream);
                stream.Flush();
                stream.Position = 0;

                var newPrototypes = (WPrototypes) serializer.Deserialize(stream, typeof(WPrototypes));
                Assert.AreEqual(newPrototypes.Affectations[0], affs[0]);
                Assert.AreEqual(newPrototypes.Affectations[1], affs[1]);
                Assert.AreEqual(newPrototypes.Prototypes[0][0], prototypes[0][0]);
                Assert.AreEqual(newPrototypes.Prototypes[0][1], prototypes[0][1]);
                Assert.AreEqual(newPrototypes.Prototypes[1][0], prototypes[1][0]);
            }
        }
    }
}
