using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IOR_SetRepository<T> where T : DistributedEntity
    {
        IEnumerable<OR_SetElement<T>> GetAdds();

        IEnumerable<OR_SetElement<T>> GetRemoves();

        void PersistAdds(IEnumerable<OR_SetElement<T>> values);

        void PersistRemoves(IEnumerable<OR_SetElement<T>> values);
    }
}