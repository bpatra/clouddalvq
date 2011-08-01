#region This code is released under the terms of the new BSD licence.
//Authors : Fabrice Rossi, Matthieu Durut
//this projects build a clustering running on Microsoft.Azure.
//the code is build on top of the open source library Lokad.Cloud
//More information at : http://lokadcloud.codeplex.com/ or http://code.google.com/p/lokad-cloud/
#endregion

using System;
using System.Runtime.Serialization;

namespace AsynchronousQuantization
{
    [Serializable, DataContract]
    public class AsynchronousSetUpMessage
    {
        /// <summary>Expiration date for the flow processing.</summary>
        [DataMember]
        public DateTimeOffset Expiration { get; set; }

        /// <summary>Number of mappers</summary>
        [DataMember]
        public int P { get; set; }

        /// <summary>Dimension of the data</summary>
        [DataMember]
        public int D { get; set; }

        /// <summary>Number of points per worker</summary>
        [DataMember]
        public int N { get; set; }

        /// <summary>Number of prototypes</summary>
        [DataMember]
        public int K { get; set; }

        /// <summary>Learning rate parameter in the gradient descent</summary>
        [DataMember]
        public double LearningRate { get; set; }

        /// <summary>Learning rate parameter in the gradient descent</summary>
        [DataMember]
        public int BatchSize { get; set; }

        public AsynchronousSetUpMessage(DateTimeOffset expiration, int p, int d, int n, int k, double learningRate, int batchSize)
        {
            Expiration = expiration;
            P = p;
            D = d;
            N = n;
            K = k;
            LearningRate = learningRate;
            BatchSize = batchSize;
        }
    }
}
