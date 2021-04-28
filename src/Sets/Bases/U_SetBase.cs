using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Bases
{
    public abstract class U_SetBase<T> where T : DistributedEntity
    {
        public IImmutableSet<U_SetElement<T>> Elements { get; protected set; }

        protected U_SetBase()
        {
            Elements = ImmutableHashSet<U_SetElement<T>>.Empty;
        }

        protected U_SetBase(IImmutableSet<U_SetElement<T>> elements)
        {
            Elements = elements;
        }

        public bool Lookup(T value) => Elements.Any(e => Equals(e.Value, value) && !e.Removed);
    }
}