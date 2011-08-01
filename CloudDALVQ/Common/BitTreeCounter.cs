#region Copyright (c) 2011--, Benoit PATRA <benoit.patra (AT) gmail (DOT) com> and Matthieu Durut <durut.matthieu (AT) gmail (DOT) com>
// This code is released under the terms of the new BSD licence.
// URL: http://code.google.com/p/clouddalvq/
#endregion


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using CloudDALVQ.Handy;
using Lokad.Cloud.Storage;
using Lokad.Cloud.Storage.Shared.Threading;

namespace CloudDALVQ.Common
{
    /// <summary>Simple sharded thread-safe idempotent tree counter shared among workers 
    /// through the blobStorage. BitTree counter provides an abstraction to hide complexity 
    /// related to performance handling.</summary>
    /// 
    /// <remarks>The usecase for this counter is synchronisation barrier between multiple 
    /// tasks to be processed. Class is designed to avoid bandwidth contention, minimize 
    /// overall latency of decrementation, and ensure idempotency (a job marked as completed 
    /// multiple times should not hurt logic). The counter is incremented once of a quantity 
    /// called "capacity", and then decremented "capacity" times. Each time the counter is 
    /// decremented, we need to check if the internal value hit 0 to remove synchronization 
    /// barrier. Thus, simple sharded counters would not have been efficient enough : because 
    /// it would have required we read all the counters each time we decrement one of the counters.
    /// 
    /// This bitTreeCounter is designed to handle efficient optimistic read-modify-write concurrent 
    /// operations by implementing a tree topology of dependant counters to avoid collisions and remove 
    /// all the contention issues while dealing with these optimistic read-modify-write operations.
    /// 
    /// Typical use of this counter is to create synchronisation barrier for jobs among a same service : 
    /// we first define the number of messages(=jobs) to be executed and set all the bit of the counters to true. 
    /// Then we run the jobs and at the end of each job, we let the counter know that
    /// the job i has been completed at least once. The bitTreeCounter then set the bit index i to false. When all 
    /// the bit have been marked as false, the bitTreeCounter returns true once and only once.
    /// The idempotency policy handles 2 scenarios : 
    /// 1) a job is run twice and we don't want the counter to be decremented twice. (Idempotency)
    /// 2) a job is run twice and we don't want the counter to hit -1 after all the decrementation 
    /// (would trigger next service twice and therefore double the workload).
    /// 
    /// 
    /// For n decrements, we have a tree depth of about log(n)/log(20). When a counter hits 0, it decrements the counter above him. 
    /// When the only counter at depth 0 hits 0, everything is completed. 
    /// </remarks>
    public class BitTreeCounter
    {
        /// <remarks>Optimized for Blob Storage latency.</remarks>
        public const int MaxValuePerCounter = 20;

        /// <summary>Maximal Depth of the BitTreeCounter. Theoretically handles up to 3.2 million decrements.</summary>
        public const int MaxDepth = 5;

        /// <summary>Container name for node entities blobNames</summary>
        private readonly string _containerName;

        /// <summary>Prefix for node entities blobNames</summary>
        private readonly string _prefix;

        /// <summary>Depth of the tree.</summary>
        private readonly int _treeDepth;

        /// <summary>Stores the number of sharded entity at this depth.</summary>
        private readonly int[] _numberOfCountersAtThisDepth;

        public BitTreeCounter(BitTreeEntity entity)
        {
            _containerName = entity.ContainerName;
            _prefix = entity.Prefix;
            _treeDepth = TreeDepth(entity.Capacity);
            _numberOfCountersAtThisDepth = DepthsLength(entity.Capacity);
        }

        /// <summary>Provides abstraction to counter decrementation. 
        /// From the outside, everything works as if we were only manipulating one mere counter.
        /// From the inside, the method decrements a counter on the deepest depth. If the counter hits 0, then we decrement 
        ///a counter on the depth above, and keep going until we hit a non "empty" counter 
        ///or the unique counter at depth 0 hits 0.</summary>
        public bool DecrementAtIndex(int outsideIndex, IBlobStorageProvider storage)
        {
            bool isLocalCounterCompleted;
            var presentDepth = _treeDepth - 1;

            do
            {
                var targetPosition = GetPosition(outsideIndex, presentDepth);
                var indexInTheCounter = IndexInTheCounter(outsideIndex, presentDepth);

                isLocalCounterCompleted = DecrementCounter(targetPosition, indexInTheCounter, storage);

                presentDepth--;
            } while (isLocalCounterCompleted && presentDepth >= 0); // if counter hits 0, we jump to upper depth.

            return isLocalCounterCompleted; //return true if we jumped till depth 0, and if the only counter at depth 0 hits 0 (happens only once).
        }

        bool DecrementCounter(TreePosition position, int insideIndex, IBlobStorageProvider blobStorage)
        {
            bool hasHitZeroForTheFirstTime = false;

            var counterAddress = BuildCounterName(position);

            Func<Maybe<NodeEntity>> insert = () =>
            {
                int counterCapacity = CounterCapacity(position);

                var counterEntity = new NodeEntity()
                {
                    Count = counterCapacity,
                    IndexAlreadyDecremented = SetupArray(counterCapacity)
                };

                bool wasAlreadyDecremented; //not used here.
                return DecrementEntity(counterEntity, insideIndex, out hasHitZeroForTheFirstTime, out wasAlreadyDecremented);
            };

            //If index has already been decremented, wasAlreadyDecremented is true, 
            //and we can skip update to avoid useless communications 
            //(update is not uploaded if we return Maybe<T>.Empty in the update function).
            Func<NodeEntity, Maybe<NodeEntity>> update = counterEntity =>
            {
                bool wasAlreadyDecremented;
                var decrementedEntity = DecrementEntity(counterEntity, insideIndex, out hasHitZeroForTheFirstTime, out wasAlreadyDecremented);
                return (!wasAlreadyDecremented) ? decrementedEntity : Maybe<NodeEntity>.Empty;
            };

            blobStorage.UpsertBlobOrSkip(_containerName, counterAddress, insert, update);

            return hasHitZeroForTheFirstTime;
        }

        /// <summary>Decrements an entity and returns a new copy of it. 
        /// Returns as side effect a boolean to indicate whether the counter 
        /// hit 0 for the first time or not, and a boolean to inform if the 
        /// method modified the entity (performance purpose)</summary>
        static NodeEntity DecrementEntity(NodeEntity counter, int index,
            out bool hasHitZeroForTheFirstTime, out bool wasAlreadyDecremented)
        {
            //Converting byte[] to BitArray to easily manipulate each bit.
            var bitArray = new BitArray(counter.IndexAlreadyDecremented);

            if (!bitArray[index])
            {
                bitArray[index] = true; //value at index 'index' is now marked as decremented
                wasAlreadyDecremented = false;

                var modifiedCounter = new NodeEntity()
                {
                    Count = counter.Count,
                    IndexAlreadyDecremented = new byte[counter.IndexAlreadyDecremented.Length]
                };

                bitArray.CopyTo(modifiedCounter.IndexAlreadyDecremented, 0);

                //return true if all indexes have been decremented. (returns true only once).
                modifiedCounter.Count--;
                hasHitZeroForTheFirstTime = (modifiedCounter.Count == 0);
                return modifiedCounter;
            }
            else
            {
                hasHitZeroForTheFirstTime = false;
                wasAlreadyDecremented = true;
                return counter;
            }
        }

        /// <summary>Remove all the created entities using this BitTreeCounter, so a new BitTreeCounter could 
        /// be used again at the same place in the blobStorage. Typical usage of BitTreeCounter 
        /// should be inside the temporary-container, using a new prefix each time. Yet, if 
        /// a counter is stored in another container using a fixed blobName, the counter should be cleaned
        /// after it hits 0. 
        /// WARNING : since blobStorage deletion is an asynchronous mechanism, counter cleaning-up will not be guaranteed 
        /// to be completed when this method ends-up.</summary>
        public void Clean(IBlobStorageProvider storage)
        {
            var countersPositions = new List<TreePosition>();
            for (int i = 0; i < _treeDepth; i++)
            {
                for (int j = 0; j < _numberOfCountersAtThisDepth[i]; j++)
                {
                    countersPositions.Add(new TreePosition() { X = j, Y = i });
                }
            }

            //Parallel deletion of blobs
            countersPositions.ToArray().SelectInParallel(
                e => storage.DeleteBlobIfExist(_containerName, BuildCounterName(e)));
        }

        /// <summary>Given an outside index, get the set of counters 
        /// that could be impacted by this index (one per depth), 
        /// and returns the position in the tree of the impacted counter at the depth : presentDepth</summary>
        TreePosition GetPosition(int outsideIndex, int presentDepth)
        {
            int x = outsideIndex;
            for (int i = _treeDepth - 1; i >= presentDepth; i--)
            {
                //We have n counters at depth i+1, and p at depth i, with n>p.
                //We need to set up an affectation mechanism between counters 
                //at depth i+1 and i, represented by f : [0:n-1] => [0,p-1]

                //Natural choice would be f(i) = i/(n/p). 
                //Yet, this choice leads to poor load balance since :
                //1)if i and j are close, f(i) and f(j) are likely to be equal.
                //2)if i and j are close, i and j are likely to be decremented at times very close.

                //Thus, we choose f(i) = i%p.
                //This choice leads to same level of stress on each counter at a given depth.

                x %= _numberOfCountersAtThisDepth[i];
            }

            return new TreePosition { X = x, Y = presentDepth };
        }

        /// <summary>Transform the outsideIndex in the insideIndex (index for the targeted counter)</summary>
        int IndexInTheCounter(int outsideIndex, int depthHeight)
        {
            int x = outsideIndex;
            for (int i = _treeDepth - 1; i > depthHeight; i--)
            {
                //Same logic than in GetPosition.
                x %= _numberOfCountersAtThisDepth[i];
            }

            return x / _numberOfCountersAtThisDepth[depthHeight];
        }

        /// <summary>Returns for a new counter the capacity it should hold.</summary>
        int CounterCapacity(TreePosition position)
        {
            var n = _numberOfCountersAtThisDepth[position.Y + 1]; //Number of counters at depth above
            var p = _numberOfCountersAtThisDepth[position.Y]; //Number of counters at upper depth

            //At a given depth, all the counters have the same amount of indexes to hold, 
            //but since n%p is not always equal to 0, the first counters at a given depth 
            //have 1 more elements than the others 
            return (position.X < n % p) ? (n / p) + 1 : (n / p);
        }

        /// <summary>
        /// We take the minimum depth for which no counter has more than MaxValuePerCounter decrements to run.
        /// </summary>
        static int TreeDepth(int capacity)
        {
            return Math.Max(Range.Array(MaxDepth).First(i => capacity < Math.Pow(MaxValuePerCounter, i)), 1);
        }

        /// <summary>
        /// The i-th depth length is Capacity^(i/D) where D is the depth of the tree.
        /// </summary>
        static int[] DepthsLength(int capacity)
        {
            int treeDepth = TreeDepth(capacity);
            return Range.Array(treeDepth + 1).Select(i => (int)Math.Ceiling(Math.Pow(capacity, ((double)i) / treeDepth))).ToArray();
        }

        /// <summary>For n indexes to decrement, we need n bits, therefore a byte[] of size n/8</summary>
        static byte[] SetupArray(int n)
        {
            return new byte[(int)Math.Ceiling((double)n / 8)];
        }

        string BuildCounterName(TreePosition position)
        {
            return _prefix + "/H" + position.X + "-" + position.Y;
        }

        /// <summary>Position of a counter in the tree</summary>
        class TreePosition
        {
            /// <summary>Index of the counter at a given depth</summary>
            public int X { get; set; }

            /// <summary>Depth in the Tree</summary>
            public int Y { get; set; }
        }

        /// <summary>Serialization purpose class for a node. Should be invisible from the outside.</summary>
        [Serializable, DataContract]
        class NodeEntity
        {
            /// <summary>For each index, holds a bit to determine if the index has already been decremented. 
            /// Holding bytes instead of bool keeps the counter small.</summary>
            [DataMember]
            public byte[] IndexAlreadyDecremented { get; set; }

            /// <summary>Count of Indexes not yet decremented.</summary>
            [DataMember]
            public int Count { get; set; }
        }
    }

    /// <summary>Persistence class for storing bitTree information in storage</summary>
    [Serializable, DataContract]
    public class BitTreeEntity
    {
        /// <summary>Capacity is the number of things we need to count.</summary>
        [DataMember]
        public int Capacity { get; set; }

        /// <summary>Container name for the node entities</summary>
        [DataMember]
        public string ContainerName { get; set; }

        /// <summary>Prefix for the node entities storage names</summary>
        [DataMember]
        public string Prefix { get; set; }

        public BitTreeEntity(int capacity, string containerName, string prefix)
        {
            Capacity = capacity;
            ContainerName = containerName;
            Prefix = prefix;
        }
    }
}
