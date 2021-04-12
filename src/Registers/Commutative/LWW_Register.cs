using System;
using CRDT.Abstractions.Entities;
using CRDT.Cluster.Entities;
using CRDT.DistributedTime.Entities;
using Newtonsoft.Json.Linq;

namespace CRDT.Registers.Commutative
{
    public sealed class LWW_Register<T> : Bases.LWW_RegisterBase<T>
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
            var valueJObject = JObject.FromObject(Value);

            valueJObject.Merge(operation.Value, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Merge,
                PropertyNameComparison = StringComparison.InvariantCultureIgnoreCase
            });

            var newValue = valueJObject.ToObject<T>();

            return new LWW_Register<T>(newValue, operation.UpdatedBy, operation.Timestamp.Value);
        }
    }
}