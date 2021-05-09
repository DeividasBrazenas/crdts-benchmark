using System.Collections.Immutable;
using Benchmarks.TestTypes;
using CRDT.Application.Interfaces;

namespace Benchmarks.Repositories
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