#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudDALVQ.Handy
{
	/// <summary>
	/// Helper methods for the <see cref="IEnumerable{T}"/>
	/// </summary>
	public static class ExtendIEnumerable
	{
        ///// <summary>
        ///// Shorthand extension method for converting enumerables into the arrays
        ///// </summary>
        ///// <typeparam name="TSource">The type of the source array.</typeparam>
        ///// <typeparam name="TTarget">The type of the target array.</typeparam>
        ///// <param name="self">The collection to convert.</param>
        ///// <param name="converter">The converter.</param>
        ///// <returns>target array instance</returns>
        public static TTarget[] ToArray<TSource, TTarget>(this IEnumerable<TSource> self,
            Func<TSource, TTarget> converter)
        {
            if (self == null) throw new ArgumentNullException("self");
            if (converter == null) throw new ArgumentNullException("converter");

            return self.Select(converter).ToArray();
        }

        /// <summary>
        /// returns distinct values from a sequence by using projecting each item to a new
        /// sequence with <paramref name="projection"/> and then selecting
        /// distinct values from it.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <typeparam name="TProjected">The type of the projected item.</typeparam>
        /// <param name="enumerable">The sequence to work with.</param>
        /// <param name="projection">The projection function.</param>
        /// <returns>sequence of distinct values</returns>
        public static IEnumerable<TItem> Distinct<TItem, TProjected>(this IEnumerable<TItem> enumerable,
            Func<TItem, TProjected> projection)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (projection == null) throw new ArgumentNullException("projection");

            return enumerable
                .Select(p => new Tuple<TItem, TProjected>(p, projection(p)))
                .Distinct(new ProjectionComparer<Tuple<TItem, TProjected>, TProjected>(p => p.Item2))
                .Select(pair => pair.Item1);
        }

        /// <summary>
        /// Slices array into array of arrays of length up to <paramref name="sliceLength"/>
        /// </summary>
        /// <typeparam name="T">Type of the items int the array</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="sliceLength">Length of the slice.</param>
        /// <returns>array of sliced arrays</returns>
        /// <exception cref="ArgumentNullException">When source array is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">When <paramref name="sliceLength"/> is invalid</exception>
        public static T[][] SliceArray<T>(this T[] array, int sliceLength)
        {
            if (array == null) throw new ArgumentNullException("array");

            if (sliceLength <= 0)
            {
                throw new ArgumentOutOfRangeException("sliceLength", "value must be greater than 0");
            }

            if (array.Length == 0)
                return new T[0][];

            int segments = array.Length / sliceLength;
            int last = array.Length % sliceLength;
            int totalSegments = segments + (last == 0 ? 0 : 1);

            var result = new T[totalSegments][];

            for (int i = 0; i < segments; i++)
            {
                var item = result[i] = new T[sliceLength];
                Array.Copy(array, i * sliceLength, item, 0, sliceLength);
            }
            if (last > 0)
            {
                var item = result[totalSegments - 1] = new T[last];
                Array.Copy(array, segments * sliceLength, item, 0, last);
            }

            return result;
        }
	}

    sealed class ProjectionComparer<TValue, TProjection> : IEqualityComparer<TValue>
    {
        readonly Func<TValue, TProjection> _projection;

        public ProjectionComparer(Func<TValue, TProjection> projection)
        {
            _projection = projection;
        }

        bool IEqualityComparer<TValue>.Equals(TValue x, TValue y)
        {
            var projectedX = _projection(x);
            var projectedY = _projection(y);

            return projectedX.Equals(projectedY);
        }

        int IEqualityComparer<TValue>.GetHashCode(TValue obj)
        {
            var projectedObj = _projection(obj);
            return projectedObj.GetHashCode();
        }
    }
}