#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;

namespace CloudDALVQ.Handy
{
	/// <summary>
	/// Helper class that allows to implement non-deterministic 
	/// reproducible testing.
	/// </summary>
	/// <remarks>
	/// Keep in mind, that this implementation is not thread-safe.
	/// </remarks>
    public static partial class Rand
    {
        static Func<int, int> NextInt;
        static Func<Func<int, int>> Activator;

        /// <summary>
        /// Resets everything to the default, using <see cref="Random"/> generator and random seed. 
        /// </summary>
        public static void ResetToDefault()
        {
            ResetToDefault(new Random().Next());
        }

        /// <summary>
        /// Resets everything to the default, using <see cref="Random"/> generator and the specified
        /// rand seed.
        /// </summary>
        /// <param name="randSeed">The rand seed.</param>
        public static void ResetToDefault(int randSeed)
        {
            Activator = () =>
            {
                var r = new Random(randSeed);
                return i => r.Next(i);
            };
            NextInt = Activator();
        }

        static Rand()
        {
            ResetToDefault();
        }

        /// <summary>
        /// Generates random value between 0 and <see cref="int.MaxValue"/> (exclusive)
        /// </summary>
        /// <returns>random integer</returns>
        public static int Next()
        {
            return NextInt(int.MaxValue);
        }

       
    }
}