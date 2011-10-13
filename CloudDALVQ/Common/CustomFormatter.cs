#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using CloudDALVQ;
using CloudDALVQ.Handy;
using Lokad.Cloud.Storage.Shared;

namespace CloudDALVQ.Common
{
    /// <summary>
    /// Formatter for compression used by the O/C mapper.
    /// </summary>
    public class CustomFormatter : IDataSerializer
    {
        object IDataSerializer.Deserialize(System.IO.Stream sourceStream, System.Type type)
        {
            if (type.GetAttributes<CustomContractAttribute>(false).Length > 0)
            {
                var binarySerializer = new BinaryFormatter();
                return binarySerializer.Deserialize(sourceStream);
            }
            else
            {
                var serializer = GetXmlSerializer(type);

                using (var decompressed = sourceStream.Decompress(true))
                using (var reader = XmlDictionaryReader.CreateBinaryReader(decompressed, XmlDictionaryReaderQuotas.Max))
                {
                    return serializer.ReadObject(reader);
                }
            }
            
        }

        public void Serialize(object instance, System.IO.Stream destinationStream)
        {
            var type = instance.GetType();

            if (type.GetAttributes<CustomContractAttribute>(false).Length > 0)
            {
                  var binarySerializer = new BinaryFormatter();
                  binarySerializer.Serialize(destinationStream, instance);
            }

            else
            {
                var serializer = GetXmlSerializer(type);

                using (var compressed = destinationStream.Compress(true))
                using (var writer = XmlDictionaryWriter.CreateBinaryWriter(compressed, null, null, false))
                {
                    serializer.WriteObject(writer, instance);
                } 
            }
        }

        static XmlObjectSerializer GetXmlSerializer(Type type)
        {
            if (type.GetAttributes<DataContractAttribute>(false).Length > 0)
            {
                return new DataContractSerializer(type);
            }

            return new NetDataContractSerializer();
        }
    }

    
}
