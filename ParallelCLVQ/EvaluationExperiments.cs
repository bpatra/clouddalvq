#region This code is released under the terms of the new BSD licence.
//Authors : Fabrice Rossi, Matthieu Durut
//this projects build a clustering running on Microsoft.Azure.
//the code is build on top of the open source library Lokad.Cloud
//More information at : http://lokadcloud.codeplex.com/ or http://code.google.com/p/lokad-cloud/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsynchronousQuantization;
using AsynchronousQuantization.DataGenerator;
using AsyncQuantization;

namespace Sandbox
{
    public class EvaluationExperiments
    {
        public static void RunOnSplines()
        {
            const int p = 1;
            const int d = 1250;
            const int k = 1000;
            const int g = 100;
            const int knotCount = 20;
            const double stepConstant = 1;
            const int batchSize = 50;
            const int minBatchCountBetween2Reads = 1;
            const int minBatchCountBetween2Writes = 1;
            const int evaluationCount = 100000;
            const int n = 1000;
            const string BasePath = @"../../../Output/";

            var generatorType = GeneratorType.OrthoSplines;

          
            var expiration = DateTimeOffset.Now.AddMinutes(2);
            TimeSpan additionalTimeForConsensus = new TimeSpan(0, 0, 20);
              var settings = new Settings(n, d, p, k, g, knotCount, stepConstant, batchSize, minBatchCountBetween2Reads, minBatchCountBetween2Writes,
                evaluationCount, expiration, additionalTimeForConsensus, generatorType);

              var generator = DataGeneratorFactory.GetGenerator(settings, 1987);

            var prototypes = generator.GetData(k);
           

            var writer = File.CreateText(BasePath + "quantifEvaluation.dat");
             
            int maxStep = 100;
            var quantization = new QuantizationEvaluator(prototypes, DataGeneratorFactory.GetGenerator(settings, 1));
            for(int s=1; s < evaluationCount; s+=maxStep)
            {
                //quantization.Generator = DataGeneratorFactory.GetGenerator(settings, s);
                quantization.EvaluateWith(maxStep);
                Console.WriteLine("s=" +s + "\t" +quantization.Count + "\t" + quantization.QuantizationError);
                writer.WriteLine(quantization.Count +"\t" + quantization.QuantizationError);
            }
           
            writer.Close();

            Console.ReadLine();
        }
    }
}
