using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IOR_SetRepository<T> where T : DistributedEntity
    {
        ImmutableHashSet<OR_SetElement<T>> GetAdds();

        ImmutableHashSet<OR_SetElement<T>> GetRemoves();

        void PersistAdds(ImmutableHashSet<OR_SetElement<T>> values);

        void PersistRemoves(ImmutableHashSet<OR_SetElement<T>> values);
    }
}