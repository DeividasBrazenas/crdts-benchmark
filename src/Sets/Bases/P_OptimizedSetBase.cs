using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Bases
{
    public abstract class P_OptimizedSetBase<T> where T : DistributedEntity
    {
        public ImmutableHashSet<P_OptimizedSetElement<T>> Elements { get; protected set; }

        protected P_OptimizedSetBase()
        {
            Elements = ImmutableHashSet<P_OptimizedSetElement<T>>.Empty;
        }

        protected P_OptimizedSetBase(ImmutableHashSet<P_OptimizedSetElement<T>> elements)
        {
            Elements = elements;
        }

        public bool Lookup(T value) => Elements.Any(e => Equals(e.Value, value) && !e.Removed);
    }
}