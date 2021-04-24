using System.Collections.Generic;
using CRDT.Core.Abstractions;

namespace CRDT.Application.Interfaces
{
    public interface IP_SetRepository<T> where T : DistributedEntity
    {
        IEnumerable<T> GetAdds();

        IEnumerable<T> GetRemoves();

        void PersistAdds(IEnumerable<T> values);

        void PersistRemoves(IEnumerable<T> values);
    }
}