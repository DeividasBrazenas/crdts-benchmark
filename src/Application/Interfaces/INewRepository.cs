using System.Collections.Generic;
using CRDT.Core.Abstractions;

namespace CRDT.Application.Interfaces
{
    public interface INewRepository<T> where T : DistributedEntity
    {
        IEnumerable<T> GetValues();

        void AddValue(T value);

        void AddValues(IEnumerable<T> values);
    }
}