using Abstractions.Entities;
using Cluster.Entities;

namespace Abstractions
{
    public abstract class CRDT<T> where T : DistributedEntity
    {
        public T Value { get; }

        public Node UpdatedBy { get; }

        protected CRDT(T value, Node updatedBy)
        {
            Value = value;
            UpdatedBy = updatedBy;
        }
    }
}