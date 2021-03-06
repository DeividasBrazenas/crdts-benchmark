using System;
using System.Collections.Immutable;
using System.Linq;
using Benchmarks.TestTypes;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;

namespace Benchmarks.Repositories
{
    public class OUR_OptimizedSetWithVCRepository : IOUR_OptimizedSetWithVCRepository<TestType>
    {
        public ImmutableHashSet<OUR_OptimizedSetWithVCElement<TestType>> Elements { get; private set; }

        public OUR_OptimizedSetWithVCRepository()
        {
            Elements = ImmutableHashSet<OUR_OptimizedSetWithVCElement<TestType>>.Empty;
        }

        public ImmutableHashSet<OUR_OptimizedSetWithVCElement<TestType>> GetElements() => Elements;

        public ImmutableHashSet<OUR_OptimizedSetWithVCElement<TestType>> GetElements(Guid id)
        {
            return Elements.Where(e => e.ValueId == id).ToImmutableHashSet();
        }

        public void PersistElements(ImmutableHashSet<OUR_OptimizedSetWithVCElement<TestType>> elements)
        {
            Elements = elements;
        }
    }
}