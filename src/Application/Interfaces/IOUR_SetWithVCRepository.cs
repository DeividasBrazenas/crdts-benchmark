using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IOUR_SetWithVCRepository<T> where T : DistributedEntity
    {
        IEnumerable<OUR_SetWithVCElement<T>> GetAdds();

        IEnumerable<OUR_SetWithVCElement<T>> GetRemoves();

        void PersistAdds(IEnumerable<OUR_SetWithVCElement<T>> values);

        void PersistRemoves(IEnumerable<OUR_SetWithVCElement<T>> values);
    }
}