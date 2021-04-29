using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface ILWW_OptimizedSetRepository<T> where T : DistributedEntity
    {
        IEnumerable<LWW_OptimizedSetElement<T>> GetElements();

        void PersistElements(IEnumerable<LWW_OptimizedSetElement<T>> elements);
    }
}