using CRDT.Core.Abstractions;
using CRDT.Core.Cluster;

namespace CRDT.Registers.Bases
{
    public abstract class LWW_RegisterBase<T> : ValueObject where T : DistributedEntity
    {
        public T Value { get; }

        public Node UpdatedBy { get; }

        protected LWW_RegisterBase(T value, Node updatedBy)
        {
            Value = value;
            UpdatedBy = updatedBy;
        }
    }
}