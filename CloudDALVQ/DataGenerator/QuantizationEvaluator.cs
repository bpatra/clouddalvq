#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudDALVQ;

namespace CloudDALVQ.DataGenerator
{
    public class QuantizationEvaluator
    {
            private int _maxChunkSize;

            /// <summary>Return Quantization Mean Error </summary>
            public double QuantizationError { get { return Count > 0 ? _sum / Count : 0; } }
            public double Variance
            {
                get
                {
                    return Count > 0 ? (_sumSquare/Count) - (_sum/Count)*(_sum/Count): 0;
                }
            }

            private double _sum;
            private double _sumSquare;

            public int Count { get; set;}

            private readonly IGenerator _generator;
            private readonly double[][] _prototypes;

            public QuantizationEvaluator(double[][] prototypes, IGenerator generator)
            {
                _generator = generator;
                _prototypes = prototypes;
                 _maxChunkSize = 100000000 / (prototypes[0].Length * 8); // we don't want to process at once more than 100MB of data
            }

            public void EvaluateWith(int N)
            {
              
                //This loop ensures we won't process more than 100MB at once.
                for (int chunkIterator = 0; chunkIterator <= N/_maxChunkSize; chunkIterator++)
                {
                    var chunkSize = chunkIterator != (N / _maxChunkSize) ? _maxChunkSize : N % _maxChunkSize;
                    
                    //Generates data for evaluation of some given prototypes
                    var evaluationData = _generator.GetData(chunkSize);

                    for (int n = 0; n < chunkSize; n++)
                    {
                        var point = evaluationData[n];
                        double distanceMin;

                        Util.NearestPrototype(point, _prototypes, out distanceMin);

                        _sum += distanceMin;
                        _sumSquare += distanceMin*distanceMin;
                        Count++;
                    }
                }
            }
        }

     public class QuantizationEvaluator2
    {
            /// <summary>Return Quantization Mean Error </summary>
            public double QuantizationError { get { return Count > 0 ? _sum / Count : 0; } }
            public double Variance
            {
                get
                {
                    return Count > 0 ? (_sumSquare/Count) - (_sum/Count)*(_sum/Count): 0;
                }
            }


            private double _sum;
            private double _sumSquare;

            public int Count { get; set;}

            private readonly double[][] _points;
            private readonly double[][] _prototypes;

            public QuantizationEvaluator2(double[][] prototypes, double[][] points)
            {
                _points = points;
                _prototypes = prototypes;
            }

            public void Evaluate()
            {
             //Generates data for evaluation of some given prototypes
                    var evaluationData = _points;

                    for (int n = 0; n < evaluationData.Length;n++)
                    {
                        var point = evaluationData[n];
                        double distanceMin;

                        Util.NearestPrototype(point, _prototypes, out distanceMin);

                        _sum += distanceMin;
                        _sumSquare += distanceMin*distanceMin;
                        Count++;
                    }
                }
            
        }
}
