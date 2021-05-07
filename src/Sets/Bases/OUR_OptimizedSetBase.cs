using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Bases
{
    public abstract class OUR_OptimizedSetBase<T> where T : DistributedEntity
    {
        public ImmutableHashSet<OUR_OptimizedSetElement<T>> Elements { get; protected set; }

        protected OUR_OptimizedSetBase()
        {
            Elements = ImmutableHashSet<OUR_OptimizedSetElement<T>>.Empty;
        }

        protected OUR_OptimizedSetBase(ImmutableHashSet<OUR_OptimizedSetElement<T>> elements)
        {
            Elements = elements;
        }

        public ImmutableHashSet<OUR_OptimizedSetElement<T>> ValidElements =>
            Elements
                .Where(e => !e.Removed)
                .Distinct()
                .ToImmutableHashSet();

        public ImmutableHashSet<T> Values =>
            ValidElements
                .Select(e => e.Value)
                .ToImmutableHashSet();

        public bool Lookup(T value) => Values.Any(v => Equals(v, value));
    }
}