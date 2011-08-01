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
        public void EvaluationTest()
        {
            const int batchSize = 1000;
            int k = 5;
            int n = 100000;
            int d = 2;
            var miniBatch = new double[batchSize][];

            var dataGenerator = new UniformGenerator(d, 1.0, 31321); //HACK : [durut] DataGenerator hard-coded

            var settings = new Settings(n, d, 1, k, 30, 100, 5, batchSize, 10, 25000, 0.5, new TimeSpan(1, 0, 0),
                                        new TimeSpan(1, 0, 0), GeneratorType.UniformInHyperCube, 34, false, true, true, true);
            var localProtos = ProcessService.Initialization(settings, 15);

            var processor = new Processor();
            var scheduler = new SamplingScheduler(0);

            var points = dataGenerator.GetData(n);
            var prototypes = new WPrototypes()
                                 {Prototypes = points.Take(k).ToArray(), Affectations = Range.Array(k).ToArray(i => 1)};
            int batchCount = 0;
            for (int i = 0; i < 100;i++ )
            {
                scheduler.MakeBatch(points, ref miniBatch);

                //Processing the new points
                processor.ProcessMiniBatch(miniBatch, ref localProtos, batchCount, 1.0);
                batchCount++;
            }

            var generator = new UniformGenerator(d, 1.0, Rand.Next()); //Hack : [durut] DataGenerator hard-coded
            var evaluator = new QuantizationEvaluator(prototypes.Prototypes, generator);
        
            evaluator.EvaluateWith(10000000); 
        }

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
