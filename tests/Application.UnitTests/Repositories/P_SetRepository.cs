using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Application.UnitTests.Repositories
{
    public class P_SetRepository : IP_SetRepository<TestType>
    {
        public ImmutableHashSet<TestType> Adds { get; private set; }
        public ImmutableHashSet<TestType> Removes { get; private set; }

        public P_SetRepository()
        {
            Adds = ImmutableHashSet<TestType>.Empty;
            Removes = ImmutableHashSet<TestType>.Empty;
        }

        public ImmutableHashSet<TestType> GetAdds() => Adds;

        public ImmutableHashSet<TestType> GetRemoves() => Removes;

        public void PersistAdds(ImmutableHashSet<TestType> values)
        {
            Adds = values;
        }

        public void PersistRemoves(ImmutableHashSet<TestType> values)
        {
            Removes = values;
        }
    }
}