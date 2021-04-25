using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Convergent
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

        public OUR_Set<T> Add(OUR_SetElement<T> element) => new(Adds.Add(element), Removes);

        public OUR_Set<T> Update(OUR_SetElement<T> element)
        {
            var elementToUpdate = Adds
                .Where(a => a.Value.Id == element.Value.Id && a.Tag == element.Tag)
                .OrderBy(a => a.Timestamp)
                .LastOrDefault();

            if(elementToUpdate is null || elementToUpdate?.Timestamp > element.Timestamp)
            {
                return this;
            }

            var adds = Adds.Remove(elementToUpdate);
            adds = adds.Add(element);

            return new(adds, Removes);
        }

        public OUR_Set<T> Remove(OUR_SetElement<T> element)
        {
            var elementToRemove = Adds
                .Where(a => Equals(a.Value, element.Value) && a.Tag == element.Tag)
                .OrderBy(a => a.Timestamp)
                .LastOrDefault();

            if (elementToRemove is null || elementToRemove?.Timestamp > element.Timestamp)
            {
                return this;
            }

            return new(Adds, Removes.Add(element));
        }

        public OUR_Set<T> Merge(IImmutableSet<OUR_SetElement<T>> adds, IImmutableSet<OUR_SetElement<T>> removes) => new(Adds.Union(adds), Removes.Union(removes));
    }
}