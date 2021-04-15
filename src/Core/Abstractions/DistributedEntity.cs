using System;

namespace CRDT.Core.Abstractions
{
    public abstract class DistributedEntity : ValueObject
    {
        public Guid Id { get; }

        protected DistributedEntity(Guid id)
        {
            Id = id;
        }
    }
}