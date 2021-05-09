using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IP_OptimizedSetRepository<T> where T : DistributedEntity
    {
        ImmutableHashSet<P_OptimizedSetElement<T>> GetElements();

        void PersistElements(ImmutableHashSet<P_OptimizedSetElement<T>> values);
    }
}