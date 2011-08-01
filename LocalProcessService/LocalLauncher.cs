#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CloudDALVQ;
using CloudDALVQ.BlobNames;
using CloudDALVQ.Messages;
using CloudDALVQ.Template;
using Lokad.Cloud;
using Lokad.Cloud.Runtime;
using Lokad.Cloud.Storage;

namespace LocalProcessService
{
    public class LocalLauncher
    {
        public static void Run()
        {
            #region
            //var accountName = "durut";
            //var accountKey = "4+G0RgTv1tPnUbjsQsxArbskQ2DdqHHDz3jYUZ/I6XtsmucEviYeiOYgTfhViaWp+20sPNQAlv0bADWnFTUOpg==";
            //var azureProviders = Standalone.CreateProviders("DefaultEndpointsProtocol=http;AccountName=" + accountName + ";AccountKey=" + accountKey);
            //var blobStorage = azureProviders.BlobStorage;
            #endregion

            var blobStorage = new LatencyMemoryProvider(new TimeSpan(0, 0, 1), new TimeSpan(0, 0, 0, 500));
            var processingService = new LocalProcessService(blobStorage);

            var settings = OrthoMixtureTemplate.Create();
            blobStorage.PutBlob(SettingsName.Default, settings);

            // Shared version is initialized. 2006 WC finals as seed.
            var sharedPrototypes = settings.SameInitialisation ? ProcessService.Initialization(settings, 19934 + settings.Seed)
                : ProcessService.Initialization(settings, 2006 + settings.Seed);
            Console.WriteLine("Start pushing version in storage.");
            var watch = Stopwatch.StartNew();
            blobStorage.PutBlob(new SharedWPrototypesName(settings.Expiration), sharedPrototypes);
            
            Console.WriteLine(watch.Elapsed.TotalSeconds);
            var processMessage = new ProcessMessage("0", "0", settings.Seed);
            processingService.Start(processMessage);
            Console.WriteLine("Job done");
            Console.ReadLine();
        }
    }
}
