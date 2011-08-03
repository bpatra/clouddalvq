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
using CloudDALVQ.Template;

namespace LocalProcessService
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = OrthoMixtureLowScaleTemplate.Create();
            var mS = new[] { 1, 2, 10};
            var tauS = new[] { 1 , 10, 100, 200};
            foreach (var tau in tauS)
            {
                foreach (var m in mS)
                {
                    settings.M = m;
                    settings.PushPeriods = tau;

                    var execution = new DelayedGradientParallelExecution();
                    execution.Start(settings);
                }
            }
        }
    }
}
