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
    public class UniformHyperCubeGenerator : IDataGenerator
    {
        readonly Random _rand;

        public UniformHyperCubeGenerator(int n)
        {
            _rand = new Random(n);
        }

        public UniformHyperCubeGenerator()
        {
            _rand = new Random(0);
        }

        public double[][] Generate(int n, int d)
        {
            var array = new double[n][];
            var generator = new MersenneTwister();
            for (int i = 0; i < n; i++)
            {
                array[i] = new double[d];
                for (int j = 0; j < d; j++)
                {
                    array[i][j] = generator.NextDouble();
                }
            }
            return array;
        }
    }
}
