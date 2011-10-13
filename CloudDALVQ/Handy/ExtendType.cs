#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Reflection;

namespace CloudDALVQ.Handy
{
	/// <summary>
	/// Helper related to the <see cref="Type"/>.
	/// </summary>
	public static class ExtendType
	{
		///<summary>
		/// Extension method to retrieve attributes from the type.
		///</summary>
		///<param name="target">Type to perform operation upon</param>
		///<param name="inherit"><see cref="MemberInfo.GetCustomAttributes(Type,bool)"/></param>
		///<typeparam name="T">Attribute to use</typeparam>
		///<returns>Empty array of <typeparamref name="T"/> if there are no attributes</returns>
		public static T[] GetAttributes<T>(this ICustomAttributeProvider target, bool inherit) where T : Attribute
		{
			if (target.IsDefined(typeof (T), inherit))
			{
				return target
					.GetCustomAttributes(typeof (T), inherit)
					.ToArray(a => (T) a);
			}
			return new T[0];
		}
	}
}