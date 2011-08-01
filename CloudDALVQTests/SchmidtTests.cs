#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CloudDALVQ;
using CloudDALVQ.DataGenerator;
using NUnit.Framework;

namespace CloudDALVQTests
{
    [TestFixture]
    public class SchmidtTests
    {
        [Test]
        public void FirstTest()
        {
            var someArrays = new double[3][];
            someArrays[1] = new[] {1.0, 3, 2.1};
            someArrays[0] = new[] {0.4, -4.3, 1.2};
            someArrays[2] = new[] {1.2, 4.5, 6.7};

            var results = SplinesGeneratorFactory.Schmidt(someArrays, 2.0);

            Assert.AreEqual(results[0][0], 0.178, 0.001,"#A01");
            Assert.AreEqual(results[0][1], -1.919, 0.001, "#A02");
            Assert.AreEqual(results[0][2], 0.535, 0.001, "#A03");
            Assert.AreEqual(results[1][0], 0.779, 0.001, "#A04");
            Assert.AreEqual(results[1][1], 0.561, 0.001, "#A05"); 
            Assert.AreEqual(results[1][2], 1.753, 0.001, "#A06");
            Assert.AreEqual(results[2][0], -1.833, 0.001, "#A07");
            Assert.AreEqual(results[2][1], 0.052, 0.001, "#A08"); 
            Assert.AreEqual(results[2][2], 0.798, 0.001, "#A09");

        }
    }
}
