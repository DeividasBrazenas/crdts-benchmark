using System;
using CRDT.Abstractions.Bases;

namespace CRDT.Abstractions.Entities
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