using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Bases
{
    public abstract class LWW_OptimizedSetWithVCBase<T> where T : DistributedEntity
    {
        public IImmutableSet<LWW_OptimizedSetWithVCElement<T>> Elements { get; protected set; }

        protected LWW_OptimizedSetWithVCBase()
        {
            Elements = ImmutableHashSet<LWW_OptimizedSetWithVCElement<T>>.Empty;
        }

        protected LWW_OptimizedSetWithVCBase(IImmutableSet<LWW_OptimizedSetWithVCElement<T>> elements)
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