#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using CloudDALVQ.Common;
using CloudDALVQ.DataGenerator;
using CloudDALVQ;
using CloudDALVQ.Entities;

namespace ClientApp
{
    public class DumpExperiments
    {
        public static void LocalTest(Settings settings)
        {
            int n = settings.N;
            const string BasePath = @"../../../Output/";

             var generator = DataGeneratorFactory.GetGenerator(settings, 1987);
            var scheduler = new SamplingScheduler(0);
            var data = generator.GetData(n);

            var prototypes = ProcessService.Initialization(settings, 12);

            Console.WriteLine("data generated");
            var miniBatch = new double[settings.BatchSize][];
            var processor = new Processor();
            
            Console.WriteLine("start initialization");

            int batchCount = 0;
            //Console.WriteLine("start iteration");
            while (prototypes.Affectations.Sum() <= 4000 + 4080)
            {
                //Selecting points from local data that will be processed
                scheduler.MakeBatch(data, ref miniBatch);
                //Processing the new points
                Console.WriteLine(prototypes.Affectations.Sum());
                processor.ProcessMiniBatch(miniBatch, ref prototypes, batchCount, 1.0);
                batchCount++;
            }
            var writer = File.CreateText(BasePath + "sample.dat");
            Util.WritePrototype(prototypes.Prototypes, writer);
            writer.Close();
            Console.WriteLine("job done");
            Console.ReadLine();
        }
    }
}
