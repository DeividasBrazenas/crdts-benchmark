using Cluster.Entities;
using CRDT.DistributedTime.Entities;

namespace LWW_Register.StateBased
{
    public sealed class LWW_Register<T>
    {
        private T _value;

        public Node UpdatedBy { get; }

        public Timestamp Timestamp { get; }

        public LWW_Register(T value, Node updatedBy)
        {
            _value = value;
            UpdatedBy = updatedBy;
            Timestamp = new Timestamp();
        }

        public LWW_Register(T value, Node updatedBy, long timestamp)
        {
            _value = value;
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