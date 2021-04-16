using System;
using System.Collections.Generic;
using System.Linq;
using CRDT.Application.Entities;
using CRDT.Application.Interfaces;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Application.UnitTests.Repositories
{
    public class TestTypeRepository : IRepository<TestType>
    {
        public HashSet<PersistenceEntity<TestType>> Entities { get; private set; }

        public TestTypeRepository()
        {
            Entities = new HashSet<PersistenceEntity<TestType>>();
        }

        public PersistenceEntity<TestType> GetValue(Guid id)
        {
            return Entities.FirstOrDefault(e => e.Id == id);
        }

        public void AddValue(PersistenceEntity<TestType> value)
        {
            Entities.Add(value);
        }

        public void ReplaceValue(Guid id, PersistenceEntity<TestType> newValue)
        {
            var entityToDelete = GetValue(id);

            if (entityToDelete is not null)
            {
                Entities.Remove(entityToDelete);
            }

            AddValue(newValue);
        }
    }
}