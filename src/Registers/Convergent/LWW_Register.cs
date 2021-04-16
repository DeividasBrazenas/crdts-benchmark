using CRDT.Core.Abstractions;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;

namespace CRDT.Registers.Convergent
{
    public sealed class LWW_Register<T> where T : DistributedEntity
    {
        public T Value { get; }

        public Node UpdatedBy { get; }

        public Timestamp Timestamp { get; }

        public LWW_Register(T value, Node updatedBy, long timestamp)
        {
            Value = value;
            UpdatedBy = updatedBy;
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