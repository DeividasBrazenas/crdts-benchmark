using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface ILWW_SetWithVCRepository<T> where T : DistributedEntity
    {
        IEnumerable<LWW_SetWithVCElement<T>> GetAdds();

        IEnumerable<LWW_SetWithVCElement<T>> GetRemoves();

        void PersistAdds(IEnumerable<LWW_SetWithVCElement<T>> values);

        void PersistRemoves(IEnumerable<LWW_SetWithVCElement<T>> values);
    }
}