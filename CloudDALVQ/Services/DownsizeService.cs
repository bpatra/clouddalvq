#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Storage;
using Lokad.Cloud.Storage.Shared.Logging;


namespace CloudDALVQ.Services
{

    /// <summary>
    /// This service is responsible of downsizing the number of workers used in Benchmark Flow.
    /// Since workers are used for at least one hour, since it takes time to boot workers and using a 
    /// lot of workers will probably imply next benchmark will also be big, we don't want to downsize the workers
    /// each time a benchmark is launch with less workers. We wait for some hours without "big" request to
    /// downsize the workers
    /// </summary>
    [ScheduledServiceSettings(
        AutoStart = false,
        Description = "Decrease the number of VM instances of our prototype, when not needed anymore for a while.",
        ProcessingTimeoutSeconds = 300,
        SchedulePerWorker = false,
        TriggerInterval = 600)]
    public class DownsizingService : ScheduledService
    {

        TimeSpan finishDay = new TimeSpan(19, 0, 0); //UTC+2 (france): 9pm 
        TimeSpan startDay = new TimeSpan(7, 0, 0); //UTC+2 (france): 9am 

        protected override void StartOnSchedule()
        {

            if (!Providers.Provisioning.IsAvailable)
            {
                Log.Error("DOWNSIZING: Provisioning is not available.");
                return;
            }

            var cancellationToken = new CancellationToken();
            var task = Providers.Provisioning.GetWorkerInstanceCount(cancellationToken);
            task.Wait();
            
            var currentWorkerCount = task.Result;
            if(currentWorkerCount > 1 && ((DateTime.UtcNow.TimeOfDay > finishDay) || (DateTime.UtcNow.TimeOfDay < startDay)) )
            {
                try
                {
                    var cancelToken = new CancellationToken();
                    Providers.Provisioning.SetWorkerInstanceCount(1, cancelToken);
                }
                catch (Exception e)
                {
                    Log.Error("Exception raised while downsizing" +e);
                }
            }
        }

    
    }
}
