using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Commutative
{
    public sealed class OUR_Set<T> : OUR_SetBase<T> where T : DistributedEntity
    {
        public OUR_Set()
        {
        }

        public OUR_Set(IImmutableSet<OUR_SetElement<T>> adds, IImmutableSet<OUR_SetElement<T>> removes)
            : base(adds, removes)
        {
        }

        public OUR_Set<T> Add(OUR_SetElement<T> element)
        {
            var existingElement = Adds.FirstOrDefault(a => a.Value.Id == element.Value.Id && a.Tag == element.Tag);

            if (existingElement is not null)
            {
                return Update(element);
            }

            return new(Adds.Add(element), Removes);
        }

        public OUR_Set<T> Update(OUR_SetElement<T> element)
        {
            var elementToUpdate = Adds.FirstOrDefault(a => a.Value.Id == element.Value.Id && a.Tag == element.Tag);

            if (elementToUpdate is null || elementToUpdate?.Timestamp > element.Timestamp)
            {
                return this;
            }

            var adds = Adds.Remove(elementToUpdate);
            adds = adds.Add(element);

            return new(adds, Removes);
        }

        public OUR_Set<T> Remove(OUR_SetElement<T> element)
        {
            var elementToRemove = Adds.FirstOrDefault(a => Equals(a.Value, element.Value) && a.Tag == element.Tag);

            if (elementToRemove is null || elementToRemove?.Timestamp > element.Timestamp)
            {
                return this;
            }

            return new(Adds, Removes.Add(element));
        }
    }
}