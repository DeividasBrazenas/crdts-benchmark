using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Core.Abstractions;

namespace CRDT.Application.Interfaces
{
    public interface IP_SetRepository<T> where T : DistributedEntity
    {
        ImmutableHashSet<T> GetAdds();

        ImmutableHashSet<T> GetRemoves();

        void PersistAdds(ImmutableHashSet<T> values);

        void PersistRemoves(ImmutableHashSet<T> values);
    }
}