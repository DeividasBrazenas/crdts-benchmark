using System;
using CRDT.Application.Entities;
using CRDT.Core.Abstractions;

namespace CRDT.Application.Interfaces
{
    public interface IRepository<T> where T : DistributedEntity
    {
        PersistenceEntity<T> GetValue(Guid id);

        void AddValue(PersistenceEntity<T> value);

        void ReplaceValue(Guid id, PersistenceEntity<T> newValue);
    }
}