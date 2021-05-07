using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface ILWW_OptimizedSetWithVCRepository<T> where T : DistributedEntity
    {
        ImmutableHashSet<LWW_OptimizedSetWithVCElement<T>> GetElements();

        void PersistElements(ImmutableHashSet<LWW_OptimizedSetWithVCElement<T>> elements);
    }
}