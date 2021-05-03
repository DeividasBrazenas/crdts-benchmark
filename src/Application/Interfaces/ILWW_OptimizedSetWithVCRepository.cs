using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface ILWW_OptimizedSetWithVCRepository<T> where T : DistributedEntity
    {
        IEnumerable<LWW_OptimizedSetWithVCElement<T>> GetElements();

        void PersistElements(IEnumerable<LWW_OptimizedSetWithVCElement<T>> elements);
    }
}