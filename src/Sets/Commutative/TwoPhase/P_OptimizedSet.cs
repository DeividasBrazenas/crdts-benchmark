using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Commutative.TwoPhase
{
    public sealed class P_OptimizedSet<T> : P_OptimizedSetBase<T> where T : DistributedEntity
    {
        public P_OptimizedSet()
        {
        }

        public P_OptimizedSet(ImmutableHashSet<P_OptimizedSetElement<T>> elements) : base(elements)
        {
        }

        public P_OptimizedSet<T> Add(T value)
        {
            var element = Elements.FirstOrDefault(e => e.Value.Id == value.Id);

            if (element is not null && element.Removed)
            {
                return this;
            }

            return new(Elements.Add(new P_OptimizedSetElement<T>(value, false)));
        }

        public P_OptimizedSet<T> Remove(T value)
        {
            var element = Elements.FirstOrDefault(e => e.Value.Id == value.Id);

            if (element is not null && !element.Removed)
            {
                var elements = Elements.Remove(element);

                return new(elements.Add(new P_OptimizedSetElement<T>(value, true)));
            }

            return this;
        }
    }
}