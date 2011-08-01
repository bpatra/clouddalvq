#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using CloudDALVQ.Common;

namespace CloudDALVQ.Entities
{
    [Serializable, CustomContract]
    public class WPrototypes
    {
        /// <summary>Prototypes version</summary>
        public double[][] Prototypes { get; set; }

        /// <summary></summary>
        public int[] Affectations { get; set; }

        public WPrototypes Clone()
        {
            int k = Prototypes.Length;
            int d = Prototypes[0].Length;

            var newProto = new double[k][];

            for (int i = 0; i < newProto.Length; i++)
            {
                newProto[i] = new double[d];
                Array.Copy(Prototypes[i], 0, newProto[i], 0, d);
            }

            var newAff = new int[k];
            Array.Copy(Affectations, 0, newAff, 0, k);

            return new WPrototypes() { Prototypes = newProto, Affectations = newAff };
        }

        public void Empty()
        {
            for (int k=0; k < Prototypes.Length;k++)
            {
                for (int d = 0 ; d < Prototypes[k].Length;d++)
                {
                    Prototypes[k][d] = 0;
                }
                Affectations[k] = 0;
            }
        }

        public static WPrototypes NewEmpty(int K, int D)
        {
            var prototypes = new double[K][];
            for (int k=0;k < K;k++)
            {
                prototypes[k] = new double[D];
            }
            var affectations = new int[K];
            return new WPrototypes() {Affectations = affectations, Prototypes = prototypes};
        }
    }
}
