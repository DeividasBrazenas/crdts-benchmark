using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Application.UnitTests.Repositories
{
    public class LWW_OptimizedSetWithVCRepository : ILWW_OptimizedSetWithVCRepository<TestType>
    {
        public ImmutableHashSet<LWW_OptimizedSetWithVCElement<TestType>> Elements { get; private set; }

        public LWW_OptimizedSetWithVCRepository()
        {
            Elements = ImmutableHashSet<LWW_OptimizedSetWithVCElement<TestType>>.Empty;
        }

        public ImmutableHashSet<LWW_OptimizedSetWithVCElement<TestType>> GetElements() => Elements;

        public void PersistElements(ImmutableHashSet<LWW_OptimizedSetWithVCElement<TestType>> elements)
        {
            Elements = elements;
        }
    }
}