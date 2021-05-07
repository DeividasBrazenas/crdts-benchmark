using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Benchmarks.TestTypes;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;

namespace Benchmarks.Repositories
{
    public class LWW_SetRepository : ILWW_SetRepository<TestType>
    {
        public ImmutableHashSet<LWW_SetElement<TestType>> Adds { get; private set;  }
        public ImmutableHashSet<LWW_SetElement<TestType>> Removes { get; private set;}

        public LWW_SetRepository()
        {
            Adds = ImmutableHashSet<LWW_SetElement<TestType>>.Empty;
            Removes = ImmutableHashSet<LWW_SetElement<TestType>>.Empty;
        }

        public ImmutableHashSet<LWW_SetElement<TestType>> GetAdds() => Adds;

        public ImmutableHashSet<LWW_SetElement<TestType>> GetRemoves() => Removes;

        public void PersistAdds(ImmutableHashSet<LWW_SetElement<TestType>> values)
        {
            Adds = values;
        }

        public void PersistRemoves(ImmutableHashSet<LWW_SetElement<TestType>> values)
        {
            Removes = values;
        }
    }
}