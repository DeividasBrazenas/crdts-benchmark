using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Bases
{
    public abstract class LWW_OptimizedSetBase<T> where T : DistributedEntity
    {
        public ImmutableHashSet<LWW_OptimizedSetElement<T>> Elements { get; protected set; }

        protected LWW_OptimizedSetBase()
        {
            Elements = ImmutableHashSet<LWW_OptimizedSetElement<T>>.Empty;
        }

        protected LWW_OptimizedSetBase(ImmutableHashSet<LWW_OptimizedSetElement<T>> elements)
        {
            Elements = elements;
        }

        public bool Lookup(T value)
        {
            var element = Elements.FirstOrDefault(a => Equals(a.Value, value));

            if (element is not null && !element.Removed)
            {
                return true;
            }

            return false;
        }
    }
}