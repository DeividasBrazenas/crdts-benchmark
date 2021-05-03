using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Convergent
{
    public sealed class OR_OptimizedSet<T> : OR_OptimizedSetBase<T> where T : DistributedEntity
    {
        public OR_OptimizedSet()
        {
        }

        public OR_OptimizedSet(IImmutableSet<OR_OptimizedSetElement<T>> elements)
            : base(elements)
        {
        }

        public OR_OptimizedSet<T> Merge(IImmutableSet<OR_OptimizedSetElement<T>> elements)
        {
            var union = Elements.Union(elements);

            var filteredElements =
                union.Where(ue => !union.Any(e => Equals(ue.Value, e.Value) && ue.Tag == e.Tag && !ue.Removed && e.Removed));

            return new(filteredElements.ToImmutableHashSet());
        }
    }
}