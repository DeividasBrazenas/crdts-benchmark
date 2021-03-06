using System;
using System.Collections.Immutable;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IOUR_SetWithVCRepository<T> where T : DistributedEntity
    {
        ImmutableHashSet<OUR_SetWithVCElement<T>> GetAdds();

        ImmutableHashSet<OUR_SetWithVCElement<T>> GetRemoves();

        ImmutableHashSet<OUR_SetWithVCElement<T>> GetAdds(Guid id);

        ImmutableHashSet<OUR_SetWithVCElement<T>> GetRemoves(Guid id);

        void PersistAdds(ImmutableHashSet<OUR_SetWithVCElement<T>> values);

        void PersistRemoves(ImmutableHashSet<OUR_SetWithVCElement<T>> values);
    }
}