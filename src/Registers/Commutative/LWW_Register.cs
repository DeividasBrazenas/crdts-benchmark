using System;
using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using CRDT.Registers.Operations;
using Newtonsoft.Json.Linq;

namespace CRDT.Registers.Commutative
{
    public sealed class LWW_Register<T> : Bases.LWW_RegisterBase<T>
        where T : DistributedEntity
    {
        public Timestamp Timestamp { get; }

        public LWW_Register(T value, Node updatedBy, long timestamp) : base(value, updatedBy)
        {
            Timestamp = new Timestamp(timestamp);
        }

        public LWW_Register(T value, Node updatedBy, Timestamp timestamp) : base(value, updatedBy)
        {
            Timestamp = timestamp;
        }

        public LWW_Register<T> Update(Operation operation)
        {
            if (Timestamp > operation.Timestamp)
            {
                return this;
            }
            if (Timestamp < operation.Timestamp)
            {
                return UpdateValueWithOperation(operation);
            }
            if (UpdatedBy < operation.UpdatedBy)
            {
                return this;
            }

            return UpdateValueWithOperation(operation);
        }

        private LWW_Register<T> UpdateValueWithOperation(Operation operation)
        {
            var currentValue = JObject.FromObject(Value);

            currentValue.Merge(operation.Value, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Merge,
                PropertyNameComparison = StringComparison.InvariantCultureIgnoreCase
            });

            var newValue = currentValue.ToObject<T>();

            return new LWW_Register<T>(newValue, operation.UpdatedBy, operation.Timestamp);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return Timestamp;
            yield return UpdatedBy;
        }
    }
}