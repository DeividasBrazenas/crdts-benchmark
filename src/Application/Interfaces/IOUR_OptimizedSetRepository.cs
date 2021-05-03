using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IOUR_OptimizedSetRepository<T> where T : DistributedEntity
    {
        IEnumerable<OUR_OptimizedSetElement<T>> GetElements();

        void PersistElements(IEnumerable<OUR_OptimizedSetElement<T>> elements);
    }
}