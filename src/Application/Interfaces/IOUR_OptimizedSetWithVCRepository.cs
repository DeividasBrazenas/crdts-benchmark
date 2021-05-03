using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IOUR_OptimizedSetWithVCRepository<T> where T : DistributedEntity
    {
        IEnumerable<OUR_OptimizedSetWithVCElement<T>> GetElements();

        void PersistElements(IEnumerable<OUR_OptimizedSetWithVCElement<T>> elements);
    }
}