using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Benchmarks.TestTypes;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;

namespace Benchmarks.Repositories
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