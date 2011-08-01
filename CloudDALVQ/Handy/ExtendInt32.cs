#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;

namespace CloudDALVQ.Handy
{
	/// <summary> Extensions to the <see cref="int"/> </summary>
	public static class ExtendInt32
	{
		/// <summary>Returns a <see cref="TimeSpan"/> that represents a specified number of minutes.</summary>
		/// <param name="minutes">number of minutes</param>
		/// <returns>A <see cref="TimeSpan"/> that represents a value.</returns>
		/// <example>3.Minutes()</example>
		public static TimeSpan Minutes(this int minutes)
		{
			return TimeSpan.FromMinutes(minutes);
		}

		/// <summary>
		/// Returns a <see cref="TimeSpan"/> that represents a specified number of seconds.
		/// </summary>
		/// <param name="seconds">number of seconds</param>
		/// <returns>A <see cref="TimeSpan"/> that represents a value.</returns>
		/// <example>2.Seconds()</example>
		public static TimeSpan Seconds(this int seconds)
		{
			return TimeSpan.FromSeconds(seconds);
		}

		/// <summary>
		/// Returns a <see cref="TimeSpan"/> that represents a specified number of milliseconds.
		/// </summary>
		/// <param name="milliseconds">milliseconds for this timespan</param>
		/// <returns>A <see cref="TimeSpan"/> that represents a value.</returns>
		public static TimeSpan Milliseconds(this int milliseconds)
		{
			return TimeSpan.FromMilliseconds(milliseconds);
		}
	}
}