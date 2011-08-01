#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections.Generic;
using System.Diagnostics;
using CloudDALVQ.Entities;
using Lokad;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Storage;

namespace CloudDALVQ.Services
{
    public abstract class BaseService<T> : QueueService<T>
    {
        /// <summary></summary>
        public ProfilingData Profiling { get; set; }

        //[patra] Unused methods used for profiling in an other context.
        //we still keep them here because they might be useful.
        //public void RunMethod(Action action, string actionName, DateTimeOffset expiration)
        //{
        //    var watch = Stopwatch.StartNew();
        //    action();
        //    watch.Stop();

        //    var workerId = System.Net.Dns.GetHostName();
        //    //Log.InfoFormat("worker id: " + workerId + " spent " + watch.Elapsed + " in method " + actionName);

        //    if (Profiling == null)
        //    {
        //        Profiling = new ProfilingData()
        //        {
        //            Jobs = new List<System.Tuple<string, TimeSpan>>(),
        //            WorkerId = workerId
        //        };
        //    }

        //    Profiling.Jobs.Add(new Tuple<string, TimeSpan>(actionName, watch.Elapsed));
        //}

        //public T RunMethod<T>(Func<T> method, string methodName, DateTimeOffset expiration)
        //{
        //    var watch = Stopwatch.StartNew();
        //    var result = method();
        //    watch.Stop();

        //    var workerId = System.Net.Dns.GetHostName();
        //    //Log.InfoFormat("worker id: " + workerId + " spent " + watch.Elapsed + " in method " + methodName);

        //    if (Profiling == null)
        //    {
        //        Profiling = new ProfilingData()
        //        {
        //            Jobs = new List<Tuple<string, TimeSpan>>(),
        //            WorkerId = workerId
        //        };
        //    }

        //    Profiling.Jobs.Add(new System.Tuple<string, TimeSpan>(methodName, watch.Elapsed));

        //    return result;
        //}

        //public void DumpProfile(DateTimeOffset expiration)
        //{
        //    BlobStorage.PutVersion(new ProfilingDataName(expiration), Profiling);
        //}

    }
}
