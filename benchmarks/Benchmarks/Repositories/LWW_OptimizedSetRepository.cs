using System.Collections.Immutable;
using Benchmarks.TestTypes;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;

namespace Benchmarks.Repositories
{
    public class LWW_OptimizedSetRepository : ILWW_OptimizedSetRepository<TestType>
    {
        public ImmutableHashSet<LWW_OptimizedSetElement<TestType>> Elements { get; private set; }

        public LWW_OptimizedSetRepository()
        {
            Elements = ImmutableHashSet<LWW_OptimizedSetElement<TestType>>.Empty;
        }

        public ImmutableHashSet<LWW_OptimizedSetElement<TestType>> GetElements() => Elements;

        public void PersistElements(ImmutableHashSet<LWW_OptimizedSetElement<TestType>> elements)
        {
            Elements = elements;
        }
    }
}