using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IU_SetRepository<T> where T : DistributedEntity
    {
        ImmutableHashSet<U_SetElement<T>> GetElements();

        void PersistElements(ImmutableHashSet<U_SetElement<T>> values);
    }
}