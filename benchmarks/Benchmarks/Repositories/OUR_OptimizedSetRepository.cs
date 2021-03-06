using System;
using System.Collections.Immutable;
using System.Linq;
using Benchmarks.TestTypes;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;

namespace Benchmarks.Repositories
{
    public class OUR_OptimizedSetRepository : IOUR_OptimizedSetRepository<TestType>
    {
        public ImmutableHashSet<OUR_OptimizedSetElement<TestType>> Elements { get; private set; }

        public OUR_OptimizedSetRepository()
        {
            Elements = ImmutableHashSet<OUR_OptimizedSetElement<TestType>>.Empty;
        }

        public ImmutableHashSet<OUR_OptimizedSetElement<TestType>> GetElements() => Elements;

        public ImmutableHashSet<OUR_OptimizedSetElement<TestType>> GetElements(Guid id)
        {
            return Elements.Where(e => e.ValueId == id).ToImmutableHashSet();
        }

        public void PersistElements(ImmutableHashSet<OUR_OptimizedSetElement<TestType>> elements)
        {
            Elements = elements;
        }
    }
}