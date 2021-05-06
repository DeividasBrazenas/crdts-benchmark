using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Application.UnitTests.Repositories
{
    public class U_SetRepository : IU_SetRepository<TestType>
    {
        public ImmutableHashSet<U_SetElement<TestType>> Elements { get; private set; }

        public U_SetRepository()
        {
            Elements = ImmutableHashSet<U_SetElement<TestType>>.Empty;
        }

        public ImmutableHashSet<U_SetElement<TestType>> GetElements() => Elements;

        public void PersistElements(ImmutableHashSet<U_SetElement<TestType>> values)
        {
            Elements = values;
        }
    }
}