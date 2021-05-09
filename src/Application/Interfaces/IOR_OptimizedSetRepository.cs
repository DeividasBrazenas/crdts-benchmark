using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IOR_OptimizedSetRepository<T> where T : DistributedEntity
    {
        ImmutableHashSet<OR_OptimizedSetElement<T>> GetElements();

        ImmutableHashSet<OR_OptimizedSetElement<T>> GetElements(Guid id);

        void PersistElements(ImmutableHashSet<OR_OptimizedSetElement<T>> elements);
    }
}