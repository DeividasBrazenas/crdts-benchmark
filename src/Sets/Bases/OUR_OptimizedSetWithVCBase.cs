using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Bases
{
    public abstract class OUR_OptimizedSetWithVCBase<T> where T : DistributedEntity
    {
        public ImmutableHashSet<OUR_OptimizedSetWithVCElement<T>> Elements { get; protected set; }

        protected OUR_OptimizedSetWithVCBase()
        {
            Elements = ImmutableHashSet<OUR_OptimizedSetWithVCElement<T>>.Empty;
        }

        protected OUR_OptimizedSetWithVCBase(ImmutableHashSet<OUR_OptimizedSetWithVCElement<T>> elements)
        {
            Elements = elements;
        }

        public ImmutableHashSet<T> Values =>
            Elements
                .Where(e => !e.Removed)
                .Select(e => e.Value)
                .ToImmutableHashSet();

        public bool Lookup(T value) => Values.Any(v => Equals(v, value));
    }
}