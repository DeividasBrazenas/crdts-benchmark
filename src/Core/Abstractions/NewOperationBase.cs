using CRDT.Core.Cluster;

namespace CRDT.Core.Abstractions
{
    public abstract class NewOperationBase<T> where T : DistributedEntity
    {
        public T Value { get; protected set; }

        public Node UpdatedBy { get; }

        protected NewOperationBase(T value, Node updatedBy)
        {
            Value = value;
            UpdatedBy = updatedBy;
        }
    }
}