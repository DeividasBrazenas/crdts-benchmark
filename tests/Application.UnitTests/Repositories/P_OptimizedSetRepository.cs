using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Application.UnitTests.Repositories
{
    public class P_OptimizedSetRepository : IP_OptimizedSetRepository<TestType>
    {
        public ImmutableHashSet<P_OptimizedSetElement<TestType>> Elements { get; private set; }

        public P_OptimizedSetRepository()
        {
            Elements = ImmutableHashSet<P_OptimizedSetElement<TestType>>.Empty;
        }

        public ImmutableHashSet<P_OptimizedSetElement<TestType>> GetElements() => Elements;

        public void PersistElements(ImmutableHashSet<P_OptimizedSetElement<TestType>> values)
        {
            Elements = values;
        }
    }
}