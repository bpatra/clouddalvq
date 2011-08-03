#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CloudDALVQ;
using CloudDALVQ.Messages;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Storage.Shared.Logging;

namespace CloudDALVQ.Services
{
     [QueueServiceSettings(AutoStart = true, 
        Description = "Testing purpose service")]
    public class TestService : QueueService<TestMessage>
    {
         protected override void Start(TestMessage message)
         {
             Action action1 = () => {Thread.Sleep(1000); Log.InfoFormat("End Action1, time :" +DateTimeOffset.Now); };
             Action action2 = () => { Thread.Sleep(10000); Log.InfoFormat("End Action2, time :" + DateTimeOffset.Now); };
             Action action3 = () => { Thread.Sleep(60000); Log.InfoFormat("End Action3, time :" + DateTimeOffset.Now); };

             Log.InfoFormat("Start at :" + DateTimeOffset.Now);
             Parallel.Invoke(new[]{action1, action2, action3});
             Log.InfoFormat("End at :" + DateTimeOffset.Now);
         }

         
    }
}
