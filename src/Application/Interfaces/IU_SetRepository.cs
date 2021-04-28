using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IU_SetRepository<T> where T : DistributedEntity
    {
        IEnumerable<U_SetElement<T>> GetElements();

        void PersistElements(IEnumerable<U_SetElement<T>> values);
    }
}