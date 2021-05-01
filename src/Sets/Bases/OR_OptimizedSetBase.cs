using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Bases
{
    public abstract class OR_OptimizedSetBase<T> where T : DistributedEntity
    {
        public IImmutableSet<OR_OptimizedSetElement<T>> Elements { get; protected set; }

        protected OR_OptimizedSetBase()
        {
            Elements = ImmutableHashSet<OR_OptimizedSetElement<T>>.Empty;
        }

        protected OR_OptimizedSetBase(IImmutableSet<OR_OptimizedSetElement<T>> elements)
        {
            Elements = elements;
        }

        public IImmutableSet<T> Values =>
            Elements
                .Where(a => !a.Removed)
                .Select(e => e.Value)
                .Distinct()
                .ToImmutableHashSet();

        public bool Lookup(T value) => Values.Any(v => Equals(v, value));
    }
}