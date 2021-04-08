using System;

namespace Abstractions.Entities
{
    public abstract class DistributedEntity
    {
        public Guid Id { get; }

        protected DistributedEntity(Guid id)
        {
            Id = id;
        }
    }
}