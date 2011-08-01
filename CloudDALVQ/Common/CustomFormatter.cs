#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using CloudDALVQ.Handy;
using Lokad.Cloud.Storage.Shared;
using ProtoBuf;

namespace CloudDALVQ.Common
{
    /// <summary>ProtoBuf Formatter. If object to serialize has a ProtoContract attribute, then it uses ProtoBuf, else it uses DataContract of NetDataContract.
    /// Since the IDataSerializer interface is a bit restrictive, we had to use Generic methods with type only known at runtime, which implies reflection...
    /// To avoid the cost of reflection at each time, methods already used are stored in dictionaries.</summary>
    public class CustomFormatter : IDataSerializer
    {
        private readonly MethodInfo _genericSerializeMethod;
        private readonly Dictionary<Type, MethodInfo> _serializedMethods;
        private readonly MethodInfo _genericDeserializeMethod;
        private readonly Dictionary<Type, MethodInfo> _deserializedMethods;

        public CustomFormatter()
        {
            //Dirtiest line of code of my life, but I'm still young.
            _genericSerializeMethod = (MethodInfo)typeof(ProtoBuf.Serializer).GetMember("Serialize")[0];
            _serializedMethods = new Dictionary<Type, MethodInfo>();

            _genericDeserializeMethod = typeof (ProtoBuf.Serializer).GetMethod("Deserialize", new[] {typeof (Stream)});
            _deserializedMethods = new Dictionary<Type, MethodInfo>();
        }

        public void Serialize(object instance, Stream destinationStream)
        {
            var type = instance.GetType();

            if (type.GetAttributes<CustomContractAttribute>(false).Length > 0)
            {
                var serializer = new WPrototypesSerializer();
                serializer.Serialize(instance, destinationStream);
            }

            //If ProtoBuf contract available, use Protobuf serialization
            else if (type.GetAttributes<ProtoContractAttribute>(false).Length > 0)
            {
                if (!_serializedMethods.ContainsKey(type))
                {
                    _serializedMethods.Add(type, _genericSerializeMethod.MakeGenericMethod(new[] {type}));
                }
                _serializedMethods[type].Invoke(null, new object[] { destinationStream, instance });
            }

            //Else use DataContract or NetDataContract serialization
            else
            {
                XmlObjectSerializer serializer = GetXmlSerializer(type);

                using (var compressed = destinationStream.Compress(true))
                using (var writer = XmlDictionaryWriter.CreateBinaryWriter(compressed, null, null, false))
                {
                    serializer.WriteObject(writer, instance);
                }
            }
        }

        public object Deserialize(System.IO.Stream sourceStream, Type type)
        {
            if (type.GetAttributes<CustomContractAttribute>(false).Length > 0)
            {
                var serializer = new WPrototypesSerializer();
                return serializer.Deserialize(sourceStream, type);
            }

            //ProtoBuf serializer. Because ProtoBuf is Generic and type is passed as a variable, not a generic, we 
            //are forced to use reflection. Fields are created to avoid costly method generation on each Deserialization process.
            if (type.GetAttributes<ProtoContractAttribute>(false).Length > 0)
            {
                if (!_deserializedMethods.ContainsKey(type))
                {
                    _deserializedMethods.Add(type, _genericDeserializeMethod.MakeGenericMethod(new[] { type }));
                }

                return _deserializedMethods[type].Invoke(null, new object[] { sourceStream });
            }

            else
            {
                var serializer = GetXmlSerializer(type);

                using(var decompressed = sourceStream.Decompress(true))
                using(var reader = XmlDictionaryReader.CreateBinaryReader(decompressed, XmlDictionaryReaderQuotas.Max))
                {
                    return serializer.ReadObject(reader);
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
