using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Commutative.TwoPhase
{
    public sealed class U_Set<T> : U_SetBase<T> where T : DistributedEntity
    {
        public U_Set()
        {
        }

        public U_Set(ImmutableHashSet<U_SetElement<T>> elements) : base(elements)
        {
        }

        public U_Set<T> Add(T value)
        {
            var element = Elements.FirstOrDefault(e => Equals(e.Value, value));

            if (element is not null && element.Removed)
            {
                return this;
            }

            return new(Elements.Add(new U_SetElement<T>(value, false)));
        }

        public U_Set<T> Remove(T value)
        {
            var element = Elements.FirstOrDefault(e => Equals(e.Value, value));

            if (element is not null && !element.Removed)
            {
                var elements = Elements.Remove(element);

                return new(elements.Add(new U_SetElement<T>(value, true)));
            }

            return this;
        }
    }
}