#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


namespace CloudDALVQ.Handy
{
	/// <summary>
	/// Helper class with shortcut methods for managing enumerations.
	/// Useful for inlining object generation in tests
	/// </summary>
	public static class Range
	{
		/// <summary>
		/// Creates the array of integers
		/// </summary>
		/// <param name="count">The count.</param>
		/// <returns></returns>
		public static int[] Array(int count)
		{
			var array = new int[count];
			for (var i = 0; i < array.Length; i++)
			{
				array[i] = i;
			}
			return array;
		}
	}
}