#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CloudDALVQ.DataGenerator;
using CloudDALVQ.Messages;
using CloudDALVQ;
using CloudDALVQ.Entities;
using CloudDALVQ.Template;
using Lokad.Cloud;
using ClientApp;

namespace ClientApp
{
    /// <summary>
    /// Launcher for cloud execution, sends a instance of <cref AsyncSetupMessage/> in azure hosted service.
    /// </summary>
    class Program
    {
        public const string BasePath = @"../../../Output/";
        static void Main(string[] args)
        {
            const string accountName = "durut";
            const string accountKey = "xxx";
            var providers = Standalone.CreateProviders("DefaultEndpointsProtocol=http;AccountName=" + accountName + ";AccountKey=" + accountKey);

            var settings = OrthoMixtureTemplate.Create();
            var setUpMessage = new AsyncSetupMessage(settings);
            providers.QueueStorage.Put(TypeMapper.GetStorageName(typeof(AsyncSetupMessage)), setUpMessage);
            
        }
    }
}
