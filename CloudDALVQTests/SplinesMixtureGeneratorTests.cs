#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CloudDALVQ.DataGenerator;
using CloudDALVQ.Handy;
using Lokad;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using NUnit.Framework;

namespace CloudDALVQTests
{
    [TestFixture]
    public class SplinesMixtureGeneratorTests
    {
        [Test]
        public void DrawSpline()
        {
              var eta = new double[]{2.3, 1.0, 1.2, -3.1, 9.0, -1.5};
             const int knotCount = 10;
             const int MaxEvaluation = 5000;
             var tt = Enumerable.Range(0, MaxEvaluation).ToArray(i => i * knotCount / (double)MaxEvaluation);
            var spline = new Spline(tt,  Enumerable.Range(0,knotCount).ToArray(i => (double) i));
            var eval = spline.MakeCombination(eta);

            var stream = File.CreateText(@"../../../Output/test.dat");
            for (int i = 0; i < eval.Length; i++)
            {
                stream.WriteLine(tt[i] + "\t" + eval[i]);
            }
            stream.Close();
        }


        [Test]
        public void Centers()
        {
            var stream = File.CreateText(@"../../../Output/centers.dat");
            const int d =1000;
            const int g = 1500;
            const int knotCount = 50;
           
            var generator = SplinesGeneratorFactory.OrthoMixture(g, d,knotCount, 123);
            var centers  = generator.GetCenters();
            var _knots = Range.Array(knotCount).ToArray(i => (double)i );
            var tt = Enumerable.Range(0, d).ToArray(i => i * _knots[_knots.Length - 1] / (double)d );

            for (int t = 0; t < centers[0].Length; t++)
            {
                var str = tt[t] + "\t";
                for (int i = 0; i < centers.Length; i++)
                {
                    str += centers[i][t] + "\t";
                }
                stream.WriteLine(str);
            }
            stream.Close();

        }

        [Test]
        public void DrawData()
        {
            var stream = File.CreateText(@"../../../Output/mixture.dat");
            const int d = 1000;
            const int g = 1500;
            const int knotCount = 50;

            var generator = SplinesGeneratorFactory.OrthoMixture(g, d, knotCount, 123);
            var data = generator.GetData(4);

            var _knots = Range.Array(knotCount).ToArray(i => (double)i);
            var tt = Enumerable.Range(0, d).ToArray(i => i * _knots[_knots.Length - 1] / (double)d);

            for (int t = 0; t < data[0].Length; t++)
            {
                var str = tt[t] + "\t";
                for (int i = 0; i < data.Length; i++)
                {
                    str += data[i][t] + "\t";
                }
                stream.WriteLine(str);
            }
            stream.Close();


        }

        [Test]
        public void ReDraw()
        {
            var stream = File.CreateText(@"../../../Output/complexmixture.dat");
            var generator = SplinesGeneratorFactory.OrthoMixture(15, 250, 15, 2006);


              //Get Data
            const int dataCount = 100;
            var data = generator.GetData(dataCount);

            var XLabels = generator.GetXLabel();

            var str1 = "XLable \t";
            for (int i = 0; i < data.Length; i++)
            {
                str1 += "courbe" + i + "\t";
            }
            stream.WriteLine(str1);

            for(int t =0; t < XLabels.Length; t++)
            {             
                var str = XLabels[t] + "\t";
                for(int i=0; i < data.Length; i++)
                {
                    str += data[i][t] + "\t";
                }
                stream.WriteLine(str);
            }

            stream.Close();
        }

        [Test]
        public void TestSchmidt()
        {
            var gen = new Random(1524);
            var N = 100;
            var basicVector = Range.Array(N).ToArray(k => Range.Array(N).ToArray(j => gen.NextDouble()));

            var ortho = SplinesGeneratorFactory.Schmidt(basicVector, 1.0);

            //Compute the product matrix QQT and check this matrix equals I (identity matrix).
            var product = new double[N][];
            for(int i=0; i <N; i++)
            {
                product[i] = new double[N];
                for(int j=0; j <N; j++)
                {
                    double d = 0;
                    for (int k = 0; k < N; k++)
                    {
                        d += ortho[i][k]*ortho[j][k];
                    }
                    product[i][j] = d;
                }
            }

            Assert.AreEqual(1, product[0][0],0.01,"#C01");
            Assert.AreEqual(0.0, product[1][0], 0.01, "#C02");
            Assert.AreEqual(0.0, product[N-1][N-2], 0.01, "#C03");
            Assert.AreEqual(1.0, product[N-1][N-1], 0.01, "#C04");
        }
    }
}
