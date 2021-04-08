using System;
using Abstractions.Entities;
using Abstractions.Interfaces;
using Cluster.Entities;
using CRDT.DistributedTime.Entities;
using Newtonsoft.Json.Linq;

namespace LWW_Register.Commutative
{
    public sealed class LWW_Register<T> : Abstractions.CRDT<T>, ICommutative<LWW_Register<T>>
        where T : DistributedEntity
    {
        public Timestamp Timestamp { get; }

        public LWW_Register(T value, Node updatedBy) : base(value, updatedBy)
        {
            Timestamp = new Timestamp();
        }

        public LWW_Register(T value, Node updatedBy, long timestamp) : base(value, updatedBy)
        {
            Timestamp = new Timestamp(timestamp);
        }

        public LWW_Register<T> Merge(Operation operation)
        {
            if (Timestamp > operation.Timestamp)
            {
                return this;
            }

            if (Timestamp < operation.Timestamp)
            {
                return MergeOperation(operation);
            }

            if (UpdatedBy < operation.UpdatedBy)
            {
                return this;
            }

            return MergeOperation(operation);
        }

        private LWW_Register<T> MergeOperation(Operation operation)
        {
            var jObject = JObject.FromObject(Value);

            jObject.Merge(operation.Value, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Merge,
                PropertyNameComparison = StringComparison.InvariantCultureIgnoreCase
            });

            var value = jObject.ToObject<T>();

            return new LWW_Register<T>(value, operation.UpdatedBy, operation.Timestamp.Value);
        }
    }
}