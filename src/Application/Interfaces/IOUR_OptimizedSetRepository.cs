using System;
using System.Collections.Immutable;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IOUR_OptimizedSetRepository<T> where T : DistributedEntity
    {
        ImmutableHashSet<OUR_OptimizedSetElement<T>> GetElements();

        ImmutableHashSet<OUR_OptimizedSetElement<T>> GetElements(Guid id);

        void PersistElements(ImmutableHashSet<OUR_OptimizedSetElement<T>> elements);
    }
}