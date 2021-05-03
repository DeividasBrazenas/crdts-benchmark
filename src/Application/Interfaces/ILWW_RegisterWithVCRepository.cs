using System;
using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Registers.Entities;

namespace CRDT.Application.Interfaces
{
    public interface ILWW_RegisterWithVCRepository<T> where T : DistributedEntity
    {
        IEnumerable<LWW_RegisterWithVCElement<T>> GetElements();

        LWW_RegisterWithVCElement<T> GetElement(Guid id);

        void PersistElement(LWW_RegisterWithVCElement<T> element);
    }
}