using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Application.UnitTests.Repositories
{
    public class LWW_SetWithVCRepository : ILWW_SetWithVCRepository<TestType>
    {
        public ImmutableHashSet<LWW_SetWithVCElement<TestType>> Adds { get; private set;  }
        public ImmutableHashSet<LWW_SetWithVCElement<TestType>> Removes { get; private set;}

        public LWW_SetWithVCRepository()
        {
            Adds = ImmutableHashSet<LWW_SetWithVCElement<TestType>>.Empty;
            Removes = ImmutableHashSet<LWW_SetWithVCElement<TestType>>.Empty;
        }

        public ImmutableHashSet<LWW_SetWithVCElement<TestType>> GetAdds() => Adds;

        public ImmutableHashSet<LWW_SetWithVCElement<TestType>> GetRemoves() => Removes;

        public void PersistAdds(ImmutableHashSet<LWW_SetWithVCElement<TestType>> values)
        {
            Adds = values;
        }

        public void PersistRemoves(ImmutableHashSet<LWW_SetWithVCElement<TestType>> values)
        {
            Removes = values;
        }
    }
}