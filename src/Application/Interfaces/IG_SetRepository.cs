using System.Collections.Generic;
using CRDT.Core.Abstractions;

namespace CRDT.Application.Interfaces
{
    public interface IG_SetRepository<T> where T : DistributedEntity
    {
        IEnumerable<T> GetValues();

        void PersistValues(IEnumerable<T> values);
    }
}