#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CloudDALVQ;
using CloudDALVQ.DataGenerator;
using ClientApp;
using CloudDALVQ.Entities;

namespace ClientApp
{
    public class EvaluationExperiments
    {
        public static void Run(Settings settings, int seedCount)
        {
            var evaluators = new QuantizationEvaluator[seedCount];
            var writer = File.CreateText(Program.BasePath + "quantifEvaluation.dat");
            for (int i = 0; i < seedCount; i++)
            {
                var generator = DataGeneratorFactory.GetGenerator(settings, i);
                var prototypes = generator.GetData(settings.K);
               
                evaluators[i] = new QuantizationEvaluator(prototypes, DataGeneratorFactory.GetGenerator(settings, 1000 + i));
            }
          
             
            int maxStep = 1;
            for(int s=1; s < settings.EvaluationSampleCount; s+=maxStep)
            {
                var message = evaluators[0].Count + ";";
                for (int i = 0; i < seedCount; i++)
                {
                    evaluators[i].EvaluateWith(maxStep);
                    message += evaluators[i].QuantizationError + ";";
                }
                Console.WriteLine("iteration {0} on {1}", s, settings.EvaluationSampleCount);
                writer.WriteLine(message);
            }
           
            writer.Close();
            Console.WriteLine("Jobcomplete");
            Console.ReadLine();
        }
    }
}
