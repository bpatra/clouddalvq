#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudDALVQ.Handy;
using NUnit.Framework;

namespace CloudDALVQTests
{
    [TestFixture]
    public class UtilTests
    {
        [Test]
        public void DistinctTest()
        {
            var t = new[] {new TestData() {D = 2.3}, new TestData() {D = 3.2}, new TestData() {D = 2.3}};
            var shortenedT = t.Distinct(e=>e.D);

            Assert.AreEqual(shortenedT.Count(), 2, "Distinct didn't work");
        }

        class TestData
        {
            public double D { get; set; }
        }
    }
}
