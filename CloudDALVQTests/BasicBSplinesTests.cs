#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CloudDALVQ.DataGenerator;
using CloudDALVQ;
using CloudDALVQ.Handy;
using Lokad;
using NUnit.Framework;

namespace CloudDALVQTests
{
    [TestFixture]
    public class BasicBSplinesTests
    {
        private const string Directory = @"../../../Output/";

        [Test]
        public void RunTest()
        {
            const int MaxVal = 10;
            var knots = Range.Array(MaxVal).ToArray(i => (double) i);
            var splines = new BasicSplines(knots);
            const int MaxEvaluation = 5000;
            var tt = Enumerable.Range(0, MaxEvaluation).ToArray( i => i*MaxVal/(double) MaxEvaluation);

            var evaluated = splines.Evaluation(tt);

            var writer = File.CreateText(Directory + "values.dat");
            for (int k = 0; k < evaluated[0].Length; k++)
            {
                var str = tt[k].ToString() + "\t";
                for (int i = 0; i < evaluated.Length; i++)
                {
                    str += evaluated[i][k] + "\t";
                }
                writer.WriteLine(str);
            }
            writer.Close();

        }
    
        [Test]
        public void GetTest()
        {
            int knotCount = 5000;
            const int Degree = 3;
             const int MaxEvaluation = 15500;

             var tt = Enumerable.Range(0, MaxEvaluation).ToArray(i => i * knotCount / (double)MaxEvaluation);
             var knots = Range.Array(knotCount).ToArray(i => (double) i);

            var eta = new double[knotCount - Degree - 2];
            var gen = new Random(12);
            for (int i = 0; i < eta.Length; i++)
            {
                eta[i] = gen.NextDouble()*15.0;
            }

            var watch = Stopwatch.StartNew();
            var spline = new Spline(tt, knots);
            var combin1 = spline.MakeCombination(eta);
            Console.WriteLine(watch.Elapsed.TotalSeconds);

            var watch2 = Stopwatch.StartNew();
            var spline2 = new Spline(tt,  knots);
            var combin2 = spline2.MakeCombination(eta);
            Console.WriteLine(watch2.Elapsed.TotalSeconds);

            Assert.AreEqual(combin1[15], combin2[15]);
            
        }

        //[Test]
        //public void OptimizedEvaluation()
        //{
        //    int degree = 10;
        //    int knotsCount = 515;
        //    var splines = new BasicBSpline(knotsCount, degree, 15);

        //    int mes = 18000;
        //    var tt = Range.Array(mes).ToArray(i => (double) i/mes);

        //    var watch = new Stopwatch();
        //    watch.Start();
        //    var optimized = splines.Eval(tt);
        //    Console.WriteLine("Optimized" + watch.Elapsed.TotalSeconds);
        //    watch.Reset();

        //    watch.Start();
        //    var basic = new double[tt.Length];
        //    for (int i = 0; i < tt.Length; i++ )
        //    {
        //        basic[i] = splines.Eval(tt[i]);
        //    }
        //    Console.WriteLine("Basic" + watch.Elapsed.TotalSeconds);

        //    for (int i = 0; i < tt.Length; i++)
        //    {
        //        Assert.AreEqual(basic[i], optimized[i],0.001, "B"+i);
        //    }
        //}


          
    }
}
