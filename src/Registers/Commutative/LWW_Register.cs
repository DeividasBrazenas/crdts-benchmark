using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Registers.Operations;
using Newtonsoft.Json.Linq;

namespace CRDT.Registers.Commutative
{
    public sealed class LWW_Register<T> where T : DistributedEntity
    {
        public IImmutableSet<Operation> Operations { get; }

        public LWW_Register(IImmutableSet<Operation> operations)
        {
            var elementId = operations.First().ElementId;

            if (operations.Any(o => o.ElementId != elementId))
            {
                throw new ArgumentException("All operations should have the same ElementId");
            }

            var concurrentOperationsTimestamps = new HashSet<Timestamp>();
            var validOperations = new HashSet<Operation>();

            foreach (var operation in operations)
            {
                if (operations.Any(o => Equals(operation.Timestamp, o.Timestamp)))
                {
                    concurrentOperationsTimestamps.Add(operation.Timestamp);
                }
                else
                {
                    validOperations.Add(operation);
                }
            }

            foreach (var concurrentOperationTimestamp in concurrentOperationsTimestamps)
            {
                var conflictingOperations = operations.Where(o => Equals(o.Timestamp, concurrentOperationTimestamp));

                var winner = conflictingOperations.OrderBy(o => o.UpdatedBy).First();

                validOperations.Add(winner);
            }

            Operations = validOperations.ToImmutableHashSet();
        }

        public T Value()
        {
            var valueJObject = new JObject();

            var orderedOperations = Operations.OrderBy(o => o.Timestamp);

            foreach (var operation in orderedOperations)
            {
                valueJObject.Merge(operation.Value, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Replace,
                    MergeNullValueHandling = MergeNullValueHandling.Merge,
                    PropertyNameComparison = StringComparison.InvariantCultureIgnoreCase
                });
            }

            return valueJObject.ToObject<T>();
        }
            
        public LWW_Register<T> Merge(LWW_Register<T> other)
        {
            var operations = Operations.Union(other.Operations).ToImmutableHashSet();

            return new LWW_Register<T>(operations);
        }
    }
}