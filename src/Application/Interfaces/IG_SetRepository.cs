using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Core.Abstractions;

namespace CRDT.Application.Interfaces
{
    public interface IG_SetRepository<T> where T : DistributedEntity
    {
        ImmutableHashSet<T> GetValues();

        void PersistValues(ImmutableHashSet<T> values);
    }
}