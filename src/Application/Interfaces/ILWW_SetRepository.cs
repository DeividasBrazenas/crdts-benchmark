using System.Collections.Immutable;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface ILWW_SetRepository<T> where T : DistributedEntity
    {
        ImmutableHashSet<LWW_SetElement<T>> GetAdds();

        ImmutableHashSet<LWW_SetElement<T>> GetRemoves();

        void PersistAdds(ImmutableHashSet<LWW_SetElement<T>> values);

        void PersistRemoves(ImmutableHashSet<LWW_SetElement<T>> values);
    }
}