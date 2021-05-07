using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IOUR_SetRepository<T> where T : DistributedEntity
    {
        ImmutableHashSet<OUR_SetElement<T>> GetAdds();

        ImmutableHashSet<OUR_SetElement<T>> GetRemoves();

        void PersistAdds(ImmutableHashSet<OUR_SetElement<T>> values);

        void PersistRemoves(ImmutableHashSet<OUR_SetElement<T>> values);
    }
}