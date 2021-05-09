using System;
using System.Collections.Immutable;
using System.Linq;
using Benchmarks.TestTypes;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;

namespace Benchmarks.Repositories
{
    public class OR_SetRepository : IOR_SetRepository<TestType>
    {
        public ImmutableHashSet<OR_SetElement<TestType>> Adds { get; private set; }
        public ImmutableHashSet<OR_SetElement<TestType>> Removes { get; private set; }

        public OR_SetRepository()
        {
            Adds = ImmutableHashSet<OR_SetElement<TestType>>.Empty;
            Removes = ImmutableHashSet<OR_SetElement<TestType>>.Empty;
        }

        public ImmutableHashSet<OR_SetElement<TestType>> GetAdds() => Adds;

        public ImmutableHashSet<OR_SetElement<TestType>> GetRemoves() => Removes;

        public ImmutableHashSet<OR_SetElement<TestType>> GetAdds(Guid id)
        {
            return Adds.Where(x => x.ValueId == id).ToImmutableHashSet();
        }

        public ImmutableHashSet<OR_SetElement<TestType>> GetRemoves(Guid id)
        {
            return Removes.Where(x => x.ValueId == id).ToImmutableHashSet();
        }

        public void PersistAdds(ImmutableHashSet<OR_SetElement<TestType>> values)
        {
            Adds = values;
        }

        public void PersistRemoves(ImmutableHashSet<OR_SetElement<TestType>> values)
        {
            Removes = values;
        }
    }
}