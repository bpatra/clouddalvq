#region This code is released under the terms of the new BSD licence.
//Authors : Fabrice Rossi, Matthieu Durut
//this projects build a clustering running on Microsoft.Azure.
//the code is build on top of the open source library Lokad.Cloud
//More information at : http://lokadcloud.codeplex.com/ or http://code.google.com/p/lokad-cloud/
#endregion

using System;
using AsynchronousQuantization;
using MathNet.Numerics.Random;

namespace AsynchronousQuantization
{
    /// <summary>
    /// Generate synthetic data drawn uniformaly from an hyper cube.
    /// </summary>
    public class UniformProvider : IDataProvider
    {
        private Random _source;
        private int _dim;
        private double _edge;
        
        public UniformProvider(int seed, int dimension, double edge)
        {
            _source = new MersenneTwister(seed);
            _dim = dimension;
            _edge = edge;
        }


        public double[][] GetData(int count)
        {
            var array = new double[count][];
            for (int i = 0; i < count; i++)
            {
                array[i] = new double[_dim];
                for (int j = 0; j < _dim; j++)
                {
                    array[i][j] = _source.NextDouble()* _edge;
                }
            }
            return array;
        }
    }

}
