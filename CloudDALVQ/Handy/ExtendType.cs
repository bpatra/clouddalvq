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

		/// <summary>
		/// Returns single attribute from the type.
		/// </summary>
		/// <typeparam name="T">Attribute to use</typeparam>
		/// <param name="target">Attribute provider</param>
		///<param name="inherit"><see cref="MemberInfo.GetCustomAttributes(Type,bool)"/></param>
		/// <returns><em>Null</em> if the attribute is not found</returns>
		/// <exception cref="InvalidOperationException">If there are 2 or more attributes</exception>
		public static T GetAttribute<T>(this ICustomAttributeProvider target, bool inherit) where T : Attribute
		{
			if (target.IsDefined(typeof (T), inherit))
			{
				var attributes = target.GetCustomAttributes(typeof (T), inherit);
				if (attributes.Length > 1)
				{
					throw new InvalidOperationException("More than one attribute is declared");
				}
				return (T) attributes[0];
			}
			return null;
		}
	}
}