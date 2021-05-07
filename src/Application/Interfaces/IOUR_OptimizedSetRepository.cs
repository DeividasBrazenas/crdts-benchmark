using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IOUR_OptimizedSetRepository<T> where T : DistributedEntity
    {
        ImmutableHashSet<OUR_OptimizedSetElement<T>> GetElements();

        void PersistElements(ImmutableHashSet<OUR_OptimizedSetElement<T>> elements);
    }
}