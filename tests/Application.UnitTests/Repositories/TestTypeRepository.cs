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
        public List<PersistenceEntity<TestType>> Entities { get; }

        public TestTypeRepository()
        {
            Entities = new List<PersistenceEntity<TestType>>();
        }

        public PersistenceEntity<TestType> GetValue(Guid id)
        {
            return Entities.FirstOrDefault(e => e.Id == id);
        }

        public IEnumerable<PersistenceEntity<TestType>> GetValues()
        {
            return Entities;
        }

        public void AddValues(PersistenceEntity<TestType> value)
        {
            Entities.Add(value);
        }

        public void AddValues(IEnumerable<PersistenceEntity<TestType>> values)
        {
            foreach (var value in values)
            {
                Entities.Add(value);
            }
        }

        public void ReplaceValue(Guid id, PersistenceEntity<TestType> newValue)
        {
            var entityToDelete = GetValue(id);

            if (entityToDelete is not null)
            {
                Entities.Remove(entityToDelete);
            }

            AddValues(newValue);
        }
    }
}