#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Lokad.Cloud.Storage;
using Lokad.Cloud.Storage.InMemory;

namespace LocalProcessService
{
    public class LatencyMemoryProvider : IBlobStorageProvider
    {
        private readonly MemoryBlobStorageProvider _memoryProvider;
        private readonly TimeSpan _basicLatency;
        private readonly TimeSpan _ioLatency;

        public LatencyMemoryProvider(TimeSpan basicLatency, TimeSpan ioLatency)
        {
            _memoryProvider = new MemoryBlobStorageProvider();
            _basicLatency = basicLatency;
            _ioLatency = ioLatency;
        }

        public bool CreateContainerIfNotExist(string containerName)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.CreateContainerIfNotExist(containerName);
        }

        public void DeleteAllBlobs(string containerName, string blobNamePrefix = null)
        {
            Thread.Sleep(_ioLatency);
            _memoryProvider.DeleteAllBlobs(containerName, blobNamePrefix);
        }

        public bool DeleteBlobIfExist(string containerName, string blobName)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.DeleteBlobIfExist(containerName, blobName);
        }

        public bool DeleteContainerIfExist(string containerName)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.DeleteContainerIfExist(containerName);
        }

        public Maybe<object> GetBlob(string containerName, string blobName, Type type, out string etag)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.GetBlob(containerName, blobName, type, out etag);
        }

        public Maybe<T> GetBlob<T>(string containerName, string blobName, out string etag)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.GetBlob<T>(containerName, blobName, out etag);
        }

        public Maybe<T> GetBlob<T>(string containerName, string blobName)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.GetBlob<T>(containerName, blobName);
        }

        public string GetBlobEtag(string containerName, string blobName)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.GetBlobEtag(containerName, blobName);
        }

        public Maybe<T> GetBlobIfModified<T>(string containerName, string blobName, string oldEtag, out string newEtag)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.GetBlobIfModified<T>(containerName, blobName, oldEtag, out newEtag);
        }

        public Maybe<T>[] GetBlobRange<T>(string containerName, string[] blobNames, out string[] etags)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.GetBlobRange<T>(containerName, blobNames, out etags);
        }

        public Maybe<System.Xml.Linq.XElement> GetBlobXml(string containerName, string blobName, out string etag)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.GetBlobXml(containerName, blobName, out etag);
        }

        public IEnumerable<string> ListBlobNames(string containerName, string blobNamePrefix = null)
        {
            Thread.Sleep(_basicLatency);
            return _memoryProvider.ListBlobNames(containerName, blobNamePrefix);
        }

        public IEnumerable<T> ListBlobs<T>(string containerName, string blobNamePrefix = null, int skip = 0)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.ListBlobs<T>(containerName, blobNamePrefix, skip);
        }

        public IEnumerable<string> ListContainers(string containerNamePrefix = null)
        {
            Thread.Sleep(_basicLatency);
            return _memoryProvider.ListContainers(containerNamePrefix);
        }

        public bool PutBlob(string containerName, string blobName, object item, Type type, bool overwrite, out string etag)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.PutBlob(containerName, blobName, item, type, overwrite, out etag);
        }

        public bool PutBlob<T>(string containerName, string blobName, T item, string expectedEtag)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.PutBlob(containerName, blobName, item, expectedEtag);
        }

        public bool PutBlob<T>(string containerName, string blobName, T item, bool overwrite, out string etag)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.PutBlob(containerName, blobName, item,overwrite, out etag);
        }

        public bool PutBlob<T>(string containerName, string blobName, T item, bool overwrite)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.PutBlob<T>(containerName, blobName, item, overwrite);
        }

        public void PutBlob<T>(string containerName, string blobName, T item)
        {
            Thread.Sleep(_ioLatency);
            _memoryProvider.PutBlob<T>(containerName, blobName, item);
        }

        public Result<string> TryAcquireLease(string containerName, string blobName)
        {
            Thread.Sleep(_basicLatency);
            return _memoryProvider.TryAcquireLease(containerName, blobName);
        }

        public bool TryReleaseLease(string containerName, string blobName, string leaseId)
        {
            Thread.Sleep(_basicLatency);
            return _memoryProvider.TryReleaseLease(containerName, blobName, leaseId);
        }

        public bool TryRenewLease(string containerName, string blobName, string leaseId)
        {
            Thread.Sleep(_basicLatency);
            return _memoryProvider.TryRenewLease(containerName, blobName, leaseId);
        }

        public Maybe<T> UpdateBlobIfExist<T>(string containerName, string blobName, Func<T, T> update)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.UpdateBlobIfExist(containerName, blobName, update);
        }

        public Maybe<T> UpdateBlobIfExistOrDelete<T>(string containerName, string blobName, Func<T, Maybe<T>> update)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.UpdateBlobIfExistOrDelete(containerName, blobName, update);
        }

        public Maybe<T> UpdateBlobIfExistOrSkip<T>(string containerName, string blobName, Func<T, Maybe<T>> update)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.UpdateBlobIfExistOrSkip(containerName, blobName, update);
        }

        public T UpsertBlob<T>(string containerName, string blobName, Func<T> insert, Func<T, T> update)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.UpsertBlob(containerName, blobName, insert, update);
        }

        public Maybe<T> UpsertBlobOrDelete<T>(string containerName, string blobName, Func<Maybe<T>> insert, Func<T, Maybe<T>> update)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.UpsertBlobOrDelete(containerName, blobName, insert, update);
        }

        public Maybe<T> UpsertBlobOrSkip<T>(string containerName, string blobName, Func<Maybe<T>> insert, Func<T, Maybe<T>> update)
        {
            Thread.Sleep(_ioLatency);
            return _memoryProvider.UpsertBlobOrDelete(containerName, blobName, insert, update);
        }
    }
}
