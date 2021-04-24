using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;

namespace CRDT.Registers.Convergent
{
    public sealed class LWW_Register<T> : Bases.LWW_RegisterBase<T> where T : DistributedEntity
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

        public LWW_Register<T> Merge(T value, Node updatedBy, long timestamp)
        {
            if (Equals(Value, value))
            {
                return this;
            }

            var timestampObject = new Timestamp(timestamp);

            if (Timestamp > timestampObject)
            {
                return this;
            }

            if (Timestamp < timestampObject)
            {
                return new LWW_Register<T>(value, updatedBy, timestamp);
            }

            if (UpdatedBy < updatedBy)
            {
                return this;
            }

            return new LWW_Register<T>(value, updatedBy, timestamp);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return Timestamp;
            yield return UpdatedBy;
        }
    }
}