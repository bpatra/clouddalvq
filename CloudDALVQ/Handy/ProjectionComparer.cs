#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections.Generic;

namespace CloudDALVQ.Handy
{
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