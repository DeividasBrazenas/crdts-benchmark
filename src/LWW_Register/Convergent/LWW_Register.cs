using Abstractions.Entities;
using Abstractions.Interfaces;
using Cluster.Entities;
using CRDT.DistributedTime.Entities;

namespace LWW_Register.Convergent
{
    public sealed class LWW_Register<T> : Abstractions.CRDT<T>, IConvergent<LWW_Register<T>>
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

        public LWW_Register<T> Merge(LWW_Register<T> other)
        {
            if (Timestamp > other.Timestamp)
            {
                return this;
            }

            if (Timestamp < other.Timestamp)
            {
                return other;
            }

            if (UpdatedBy < other.UpdatedBy)
            {
                return this;
            }

            return other;
        }
    }
}