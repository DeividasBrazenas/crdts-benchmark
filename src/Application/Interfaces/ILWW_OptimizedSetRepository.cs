using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface ILWW_OptimizedSetRepository<T> where T : DistributedEntity
    {
        ImmutableHashSet<LWW_OptimizedSetElement<T>> GetElements();

        void PersistElements(ImmutableHashSet<LWW_OptimizedSetElement<T>> elements);
    }
}