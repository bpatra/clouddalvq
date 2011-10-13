#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using CloudDALVQ;
using CloudDALVQ.Entities;

namespace CloudDALVQ.DataGenerator
{
    /// <summary>
    /// Allows quick instanciation of <see cref="IGenerator"/> directly from <see cref="Settings"/>.
    /// </summary>
    public static class DataGeneratorFactory
    {
        public static IGenerator GetGenerator(Settings settings, int seed)
        {
            switch (settings.GeneratorType)
            {
                case GeneratorType.UniformInHyperCube:
                    return new UniformGenerator(settings.D, 10.0, seed);

                case GeneratorType.OrthoSplines:
                    return SplinesGeneratorFactory.OrthoMixture(settings.G, settings.D, settings.KnotCount, seed);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
