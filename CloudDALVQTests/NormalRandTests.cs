#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using CloudDALVQ.DataGenerator;
using CloudDALVQ;
using NUnit.Framework;

namespace CloudDALVQTests
{
    [TestFixture]
    public class NormalRandTests
    {
        [Test]
        public void Run()
        {
            const string BasePath = @"../../../Output/";


            var mixture = SplinesGeneratorFactory.OrthoMixture(5, 1250, 50, 187);
            var data1 = mixture.GetData(10);
            var writer = File.CreateText(BasePath + "rerun"+ ".dat");
            Util.WritePrototype(data1, writer);
            writer.Close();
            Console.ReadLine();
            
        }
    }
}
