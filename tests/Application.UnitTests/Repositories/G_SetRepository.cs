using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Application.UnitTests.Repositories
{
    public class G_SetRepository : IG_SetRepository<TestType>
    {
        public ImmutableHashSet<TestType> Elements { get; private set; }

        public G_SetRepository()
        {
            Elements = ImmutableHashSet<TestType>.Empty;
        }

        public ImmutableHashSet<TestType> GetValues()
        {
            return Elements;
        }

        public void PersistValues(ImmutableHashSet<TestType> values)
        {
            Elements = values;
        }
    }
}