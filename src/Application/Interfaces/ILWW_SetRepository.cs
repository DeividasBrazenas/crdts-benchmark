using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface ILWW_SetRepository<T> where T : DistributedEntity
    {
        IEnumerable<LWW_SetElement<T>> GetAdds();

        IEnumerable<LWW_SetElement<T>> GetRemoves();

        void PersistAdds(IEnumerable<LWW_SetElement<T>> values);

        void PersistRemoves(IEnumerable<LWW_SetElement<T>> values);
    }
}