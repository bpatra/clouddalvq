#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System.Runtime.Serialization.Formatters.Binary;
using CloudDALVQ.Common;
using Autofac;
using Autofac.Builder;
using Lokad.Cloud.Storage;
using Lokad.Cloud.Storage.Shared;

namespace CloudDALVQ
{
    /// <summary>Autofac module loading parts required for this project (mostly the custom formatter).</summary>
    public class CloudDALVQModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //Formatter 
            builder.RegisterType<CustomFormatter>().As<IDataSerializer>();
        }
    }
}
