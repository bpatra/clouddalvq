#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CloudDALVQ.Common;
using CloudDALVQ.DataGenerator;
using CloudDALVQ;
using CloudDALVQ.Entities;
using CloudDALVQ.Handy;
using Lokad;
using NUnit.Framework;

namespace CloudDALVQTests
{
    [TestFixture]
    public class EvaluationTests
    {

        [Test]
        public void RunTest()
        {
            const int G = 10;
            const int knotCount = 15;
            const string BasePath = @"../../../Output/";

            var gen = SplinesGeneratorFactory.OrthoMixture(G, 250, knotCount, 1987);
            var prototypes = gen.GetCenters();

            var evaluator = new QuantizationEvaluator(prototypes, gen);

            var stream = File.CreateText(BasePath + "quantizationVariance.dat");

            for(int n=1; n < 1000; n++)
            {
                evaluator.EvaluateWith(10);
                var count = evaluator.Count;
                var quantif = evaluator.QuantizationError;
                var variance = evaluator.Variance;
                stream.WriteLine(count + "\t" + quantif + "\t" + (quantif + Math.Sqrt(variance / count)) + "\t" + (quantif - Math.Sqrt(variance / count)));
                Console.WriteLine("n={0}",n);
            }

            stream.Close();

        }
    }
}
