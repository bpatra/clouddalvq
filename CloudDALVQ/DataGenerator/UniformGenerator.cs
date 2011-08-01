#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Runtime.Serialization;
using CloudDALVQ;
using MathNet.Numerics.Random;

namespace CloudDALVQ.DataGenerator
{
    /// <summary>
    /// Generate synthetic data drawn uniformaly from an hyper cube.
    /// </summary>
    [DataContract]
    public class UniformGenerator : IGenerator
    {
        [DataMember]
        private Random _source;
        
        [DataMember]
        private readonly int _dim;
        
        [DataMember]
        private readonly double _edge;
        
        public UniformGenerator(int dimension, double edge, int seed)
        {
            _dim = dimension;
            _edge = edge;
            _source = new MersenneTwister(seed);
        }

        public double[][] GetData(int count)
        {
            var array = new double[count][];
            for (int i = 0; i < count; i++)
            {
                array[i] = Next();
            }
            return array;
        }

        public double[] Next()
        {
            var vector = new double[_dim];
            for (int j = 0; j < _dim; j++)
            {
                vector[j] = _source.NextDouble() * _edge;
            }
            return vector;
        }
     }

}
