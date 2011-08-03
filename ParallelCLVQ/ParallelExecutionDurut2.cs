using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsyncQuantization;
using AsyncQuantization.DataGenerator;
using ClientApp;

namespace Sandbox
{
    public class ParallelExecutionDurut2
    {
        private const int MaxBatchCount = 15000;
        private const string BasePath = @"../../../Output/";

        public void Start(Settings settings)
        {
            var localWriter = File.CreateText(BasePath + "localProcessor.txt");
          
            Console.WriteLine("P=" + settings.P);
            Console.WriteLine("PP=" + settings.PushPeriods);
            Console.WriteLine("K=" + settings.K);
            Console.WriteLine("Seed=" + settings.Seed);

            var data = GenData(settings);
            var multiProcessor = new MultiGradientProcessor(settings) { Data = data };
            var wPrototypes =
                Enumerable.Range(0, settings.P)
                .Select(p => ProcessService.Initialization(settings, 17))
                .ToArray();

            var lonelyWData = (DataGeneratorFactory.GetGenerator(settings, settings.Seed))
                .GetData(settings.N);
            var lonelyPrototypes = ProcessService.Initialization(settings, 17);
            var lonelyScheduler = new SamplingScheduler(0);
            var lonelyBatch = new double[settings.BatchSize][];
            var lonelyProcessor = new Processor();

            int batchcount = 0;

            var oldAggVersion = wPrototypes[0].Clone();
            var cumulatedGradients = new WPrototypes[settings.P];

           for (int p = 0; p < settings.P; p++)
           {
               cumulatedGradients[p] = new WPrototypes
                {
                    Prototypes = new double[settings.K][],
                    Affectations = new int[settings.K]
                };

               for (int k = 0; k < settings.K; k++)
               {
                    cumulatedGradients[p].Prototypes[k] = new double[settings.D];
                    for (int d = 0; d < settings.D; d++)
                    {
                        cumulatedGradients[p].Prototypes[k][d] = 0;
                    }
               }
           }

            while (batchcount < MaxBatchCount)
            {
                var gradients = multiProcessor.Process(wPrototypes);
                multiProcessor.ApplyGradient(ref wPrototypes, gradients);
                multiProcessor.SumGradients(ref cumulatedGradients, gradients);

                lonelyScheduler.MakeBatch(lonelyWData, ref lonelyBatch);
                lonelyProcessor.ProcessMiniBatch(lonelyBatch, ref lonelyPrototypes, batchcount, 1.0);

                if (batchcount == 0)
                {
                    var s = batchcount + ";";

                    var error = Evaluate(wPrototypes[0], settings, settings.Seed);
                    s += error + ";";

                    var lonelyError = Evaluate(lonelyPrototypes, settings, settings.Seed);
                    s += lonelyError + ";";

                    localWriter.WriteLine(s);
                    Console.WriteLine(s);
                }

                batchcount++;

                if (batchcount % settings.PushPeriods == 0)
                {
                    for (int p = 0; p < settings.P; p++)
                    {
                        for (int k = 0; k < settings.K; k++)
                        {
                            for (int d = 0; d < settings.D; d++)
                            {
                                wPrototypes[p].Prototypes[k][d] = oldAggVersion.Prototypes[k][d] -
                                                                  cumulatedGradients[p].Prototypes[k][d];
                            }
                        }
                    }

                    for (int p = 0; p < settings.P; p++)
                    {
                        for (int k = 0; k < settings.K; k++)
                        {
                            for (int d = 0; d < settings.D; d++)
                            {
                                oldAggVersion.Prototypes[k][d] -= cumulatedGradients[p].Prototypes[k][d];
                            }
                        }
                    }

                    for (int p = 0; p < settings.P; p++)
                    {
                        for (int k = 0; k < settings.K; k++)
                        {
                            for (int d = 0; d < settings.D; d++)
                            {
                                cumulatedGradients[p].Prototypes[k][d] = 0.0;
                            }
                        }
                    }
                }

                if (batchcount % 300 == 0)
                {
                    var s = batchcount + ";";

                    var error = Evaluate(wPrototypes[0], settings, settings.Seed);
                    s += error + ";";

                    var lonelyError = Evaluate(lonelyPrototypes, settings, settings.Seed);
                    s += lonelyError + ";";

                    localWriter.WriteLine(s);
                    Console.WriteLine(s);
                }
            }
        }

        public static double[][][] GenData(Settings settings)
        {
            var data = new double[settings.P][][];
            for (int p = 0; p < settings.P; p++)
            {
                data[p] = (DataGeneratorFactory.GetGenerator(settings, settings.Seed + p))
                .GetData(settings.N);
            }
            return data;
        }

        static double Evaluate(WPrototypes prototypes, Settings settings, int seed)
        {
            var generator = DataGeneratorFactory.GetGenerator(settings, seed);
            var evaluator1 = new QuantizationEvaluator(prototypes.Prototypes, generator);
            evaluator1.EvaluateWith(settings.N);
            var quantizationError1 = evaluator1.QuantizationError;
            return quantizationError1;
        }
    }
}
