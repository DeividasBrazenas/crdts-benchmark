using System.Collections.Immutable;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface ILWW_SetWithVCRepository<T> where T : DistributedEntity
    {
        ImmutableHashSet<LWW_SetWithVCElement<T>> GetAdds();

        ImmutableHashSet<LWW_SetWithVCElement<T>> GetRemoves();

        void PersistAdds(ImmutableHashSet<LWW_SetWithVCElement<T>> values);

        void PersistRemoves(ImmutableHashSet<LWW_SetWithVCElement<T>> values);
    }
}