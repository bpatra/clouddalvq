#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion
using System.IO;
using System.IO.Compression;

namespace CloudDALVQ.Handy
{
	/// <summary>
	/// Simple helper extensions for the <see cref="Stream"/>
	/// </summary>
	public static class ExtendStream
	{
		/// <summary>
		/// Wraps the specified stream with Compression stream
		/// </summary>
		/// <param name="stream">The stream the stream to compress.</param>
		/// <param name="leaveOpen"><c>true</c> to leave the stream open; overwise <c>false</c>.</param>
		/// <returns>compressing stream</returns>
		public static GZipStream Compress(this Stream stream, bool leaveOpen)
		{
			return new GZipStream(stream, CompressionMode.Compress, leaveOpen);
		}

		/// <summary>
		/// Wraps the stream with Decompressing stream
		/// </summary>
		/// <param name="stream">The stream to decompress.</param>
		/// <param name="leaveOpen"><c>true</c> to leave the stream open; overwise <c>false</c>.</param>
		/// <returns>decompressing stream</returns>
		public static GZipStream Decompress(this Stream stream, bool leaveOpen)
		{
			return new GZipStream(stream, CompressionMode.Decompress, leaveOpen);
		}
	}
}
