using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IOR_OptimizedSetRepository<T> where T : DistributedEntity
    {
        IEnumerable<OR_OptimizedSetElement<T>> GetElements();

        void PersistElements(IEnumerable<OR_OptimizedSetElement<T>> elements);
    }
}