#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.IO;
using CloudDALVQ.Entities;
using Lokad.Cloud.Storage.Shared;

namespace CloudDALVQ.Common
{
    public class WPrototypesSerializer : IDataSerializer
    {
        public object Deserialize(Stream sourceStream, Type type)
        {
            if (type != typeof(WPrototypes))
            {
                throw new NotImplementedException(
                    "WPrototypesSerializer should only be used to serialize WPrototypes instances");
            }

            var wprototypes = new WPrototypes();

            sourceStream.Seek(0, SeekOrigin.Begin);

            var binaryReader = new BinaryReader(sourceStream);
            var k = binaryReader.ReadInt32();
            var d = binaryReader.ReadInt32();

            var buffer = binaryReader.ReadBytes(d*k*sizeof (double) + k*sizeof (int));

            wprototypes.Affectations = new int[k];
            wprototypes.Prototypes = new double[k][];

            for (int i = 0 ; i < k;i++)
            {
                wprototypes.Prototypes[i] = new double[d];
                Buffer.BlockCopy(buffer, i*d*sizeof (double), wprototypes.Prototypes[i], 0, d*sizeof (double));
            }

            Buffer.BlockCopy(buffer, k * d * sizeof(double), wprototypes.Affectations, 0, k*sizeof(int));
            return wprototypes;
        }

        public void Serialize(object instance, Stream destinationStream)
        {
            if (instance.GetType() != typeof(WPrototypes))
            {
                throw new NotImplementedException(
                    "WPrototypesSerializer should only be used to serialize WPrototypes instances");
            }

            var prototypes = (WPrototypes)instance;
            var k = prototypes.Prototypes.Length;
            var d = prototypes.Prototypes[0].Length;

            var buffer = new byte[2* sizeof(int) + d*k*sizeof(double) + k*sizeof(int)];

            Buffer.BlockCopy(BitConverter.GetBytes(k), 0, buffer, 0, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(d), 0, buffer, sizeof(int), sizeof(int));
            
            for (int i = 0; i < k;i++ )
            {
                Buffer.BlockCopy(prototypes.Prototypes[i], 0, buffer, 2*sizeof (int) + i*d*sizeof (double), d*sizeof (double));
            }

            Buffer.BlockCopy(prototypes.Affectations, 0, buffer, 2*sizeof (int) + d*k*sizeof (double),
                                 prototypes.Affectations.Length*sizeof (int));

            destinationStream.Write(buffer,0, buffer.Length);
        }
    }
}
