#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CloudDALVQ.Entities
{
    [DataContract]
    public class Evaluation
    {
        [DataMember]
        public DateTimeOffset ObservationDate { get; set;}

        [DataMember]
        public double QuantizationError { get; set; }

        [DataMember]
        public double Variance { get; set; }

        [DataMember]
        public int[] Affectations { get; set; }

        [DataMember]
        public int SampleCount { get; set; }

        [DataMember]
        public double EvaluationDuration { get; set; }

        public static void FinalEvaluation(IEnumerable<Evaluation> evaluations, StringWriter writer)
        {
            if (evaluations.Any())
            {
                var start = evaluations.OrderBy(ev => ev.ObservationDate).First().ObservationDate;
                writer.WriteLine("PointsProcessed;QuantizationError;ObservationDate");
                foreach (var evaluation in evaluations.OrderBy(ev => ev.Affectations.Sum()))
                {
                    writer.WriteLine(evaluation.Affectations.Sum() + ";" + evaluation.QuantizationError + ";" +
                                     evaluation.ObservationDate.Subtract(start).TotalSeconds);
                }   
            }
    }

    }
}
