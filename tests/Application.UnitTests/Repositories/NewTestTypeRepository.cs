using System;
using System.Collections.Generic;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Application.UnitTests.Repositories
{
    public class NewTestTypeRepository : INewRepository<TestType>
    {
        public List<TestType> Entities { get; }

        public NewTestTypeRepository()
        {
            Entities = new List<TestType>();
        }

        public IEnumerable<TestType> GetValues()
        {
            return Entities;
        }

        public void AddValue(TestType value)
        {
            Entities.Add(value);
        }

        public void AddValues(IEnumerable<TestType> values)
        {
            foreach (var value in values)
            {
                if (!Entities.Any(e => Equals(e, value)))
                {
                    Entities.Add(value);
                }
            }
        }
    }
}