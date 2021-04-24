using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Commutative
{
    public sealed class OR_Set<T> : OR_SetBase<T> where T : DistributedEntity
    {
        public OR_Set()
        {
        }

        public OR_Set(IImmutableSet<OR_SetElement<T>> adds, IImmutableSet<OR_SetElement<T>> removes)
            : base(adds, removes)
        {
        }

        public OR_Set<T> Add(OR_SetElement<T> element) => new(Adds.Add(element), Removes);

        public OR_Set<T> Remove(OR_SetElement<T> element)
        {
            if (Adds.Any(e => Equals(e, element)))
            {
                return new(Adds, Removes.Add(element));
            }

            return this;
        }
    }
}