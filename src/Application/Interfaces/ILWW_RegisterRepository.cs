using System;
using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Registers.Entities;

namespace CRDT.Application.Interfaces
{
    public interface ILWW_RegisterRepository<T> where T : DistributedEntity
    {
        IEnumerable<LWW_RegisterElement<T>> GetElements();

        LWW_RegisterElement<T> GetElement(Guid id);

        void PersistElement(LWW_RegisterElement<T> element);
    }
}