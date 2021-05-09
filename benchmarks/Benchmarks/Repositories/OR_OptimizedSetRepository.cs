using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Benchmarks.TestTypes;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;

namespace Benchmarks.Repositories
{
    public class OR_OptimizedSetRepository : IOR_OptimizedSetRepository<TestType>
    {
        public ImmutableHashSet<OR_OptimizedSetElement<TestType>> Elements { get; private set; }

        public OR_OptimizedSetRepository()
        {
            Elements = ImmutableHashSet<OR_OptimizedSetElement<TestType>>.Empty;
        }

        public ImmutableHashSet<OR_OptimizedSetElement<TestType>> GetElements() => Elements;

        public ImmutableHashSet<OR_OptimizedSetElement<TestType>> GetElements(Guid id)
        {
            return Elements.Where(e => e.ValueId == id).ToImmutableHashSet();
        }

        public void PersistElements(ImmutableHashSet<OR_OptimizedSetElement<TestType>> elements)
        {
            Elements = elements;
        }
    }
}