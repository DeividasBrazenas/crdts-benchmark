using System;
using System.Collections.Generic;
using CRDT.Application.Entities;
using CRDT.Core.Abstractions;

namespace CRDT.Application.Interfaces
{
    public interface IRepository<T> where T : DistributedEntity
    {
        PersistenceEntity<T> GetValue(Guid id);

        IEnumerable<PersistenceEntity<T>> GetValues();

        void AddValues(PersistenceEntity<T> value);

        void AddValues(IEnumerable<PersistenceEntity<T>> values);

        void ReplaceValue(Guid id, PersistenceEntity<T> newValue);
    }
}