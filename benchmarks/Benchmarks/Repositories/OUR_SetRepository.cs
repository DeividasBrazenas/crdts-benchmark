using System;
using System.Collections.Immutable;
using System.Linq;
using Benchmarks.TestTypes;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;

namespace Benchmarks.Repositories
{
    public class OUR_SetRepository : IOUR_SetRepository<TestType>
    {
        public ImmutableHashSet<OUR_SetElement<TestType>> Adds { get; private set; }
        public ImmutableHashSet<OUR_SetElement<TestType>> Removes { get; private set; }

        public OUR_SetRepository()
        {
            Adds = ImmutableHashSet<OUR_SetElement<TestType>>.Empty;
            Removes = ImmutableHashSet<OUR_SetElement<TestType>>.Empty;
        }

        public ImmutableHashSet<OUR_SetElement<TestType>> GetAdds() => Adds;

        public ImmutableHashSet<OUR_SetElement<TestType>> GetRemoves() => Removes;

        public ImmutableHashSet<OUR_SetElement<TestType>> GetAdds(Guid id)
        {
            return Adds.Where(x => x.ValueId == id).ToImmutableHashSet();
        }

        public ImmutableHashSet<OUR_SetElement<TestType>> GetRemoves(Guid id)
        {
            return Removes.Where(x => x.ValueId == id).ToImmutableHashSet();
        }

        public void PersistAdds(ImmutableHashSet<OUR_SetElement<TestType>> values)
        {
            Adds = values;
        }

        public void PersistRemoves(ImmutableHashSet<OUR_SetElement<TestType>> values)
        {
            Removes = values;
        }
    }
}