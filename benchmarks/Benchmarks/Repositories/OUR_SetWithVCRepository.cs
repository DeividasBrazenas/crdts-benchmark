using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Benchmarks.TestTypes;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;

namespace Benchmarks.Repositories
{
    public class OUR_SetWithVCRepository : IOUR_SetWithVCRepository<TestType>
    {
        public ImmutableHashSet<OUR_SetWithVCElement<TestType>> Adds { get; private set; }
        public ImmutableHashSet<OUR_SetWithVCElement<TestType>> Removes { get; private set; }

        public OUR_SetWithVCRepository()
        {
            Adds = ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty;
            Removes = ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty;
        }

        public ImmutableHashSet<OUR_SetWithVCElement<TestType>> GetAdds() => Adds;

        public ImmutableHashSet<OUR_SetWithVCElement<TestType>> GetRemoves() => Removes;

        public void PersistAdds(ImmutableHashSet<OUR_SetWithVCElement<TestType>> values)
        {
            Adds = values;
        }

        public void PersistRemoves(ImmutableHashSet<OUR_SetWithVCElement<TestType>> values)
        {
            Removes = values;
        }
    }
}