#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.IO;
using CloudDALVQ;

namespace CloudDALVQ
{
	public class Util
	{
		public static int NearestPrototype(double[] data, double[][] prototypes, out double minDist)
		{
			minDist = double.MaxValue;
			int minIndex = int.MaxValue;

			for (int i = 0; i < prototypes.Length; i++)
			{
				var getDist = Distance(data, prototypes[i]);
				if (getDist < minDist)
				{
					minIndex = i;
					minDist = getDist;
				}
			}

			return minIndex;
		}

		public static double Distance(double[] item1, double[] item2)
		{
			double dist = 0;
			for (int j = 0; j < item1.Length; j++)
			{
				dist += (item1[j] - item2[j]) * (item1[j] - item2[j]);
			}
			return dist;
		}


		public static double[] Average(double[][] points)
		{
			var sum = new double[points[0].Length];

			for (int j = 0; j < sum.Length; j++)
			{
				for (int i = 0; i < points.Length; i++)
				{
					sum[j] += points[i][j];
				}
				sum[j] /= points.Length;
			}

			return sum;
		}

		public static void WritePrototype(double[][] prototype, StreamWriter writer)
		{
			for (int t = 0; t < prototype[0].Length; t++)
			{
				var str = "";
				for (int i = 0; i < prototype.Length; i++)
				{
					str += prototype[i][t] + "\t";
				}
				writer.WriteLine(str);
			}
		}

       
	}
}
