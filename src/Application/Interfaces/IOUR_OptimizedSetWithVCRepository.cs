using System;
using System.Collections.Immutable;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IOUR_OptimizedSetWithVCRepository<T> where T : DistributedEntity
    {
        ImmutableHashSet<OUR_OptimizedSetWithVCElement<T>> GetElements();

        ImmutableHashSet<OUR_OptimizedSetWithVCElement<T>> GetElements(Guid id);

        void PersistElements(ImmutableHashSet<OUR_OptimizedSetWithVCElement<T>> elements);
    }
}