using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IOUR_SetRepository<T> where T : DistributedEntity
    {
        IEnumerable<OUR_SetElement<T>> GetAdds();

        IEnumerable<OUR_SetElement<T>> GetRemoves();

        void PersistAdds(IEnumerable<OUR_SetElement<T>> values);

        void PersistRemoves(IEnumerable<OUR_SetElement<T>> values);
    }
}