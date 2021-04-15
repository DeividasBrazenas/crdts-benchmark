using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Convergent
{
    public sealed class LWW_Set<T> : LWW_SetBase<T> where T : DistributedEntity
    {
        public LWW_Set(IImmutableSet<LWW_SetElement<T>> adds, IImmutableSet<LWW_SetElement<T>> removes)
            : base(adds, removes)
        {
        }

        public LWW_Set<T> Add(LWW_SetElement<T> element)
        {
            var existingElement = Adds.FirstOrDefault(e => e.Value.Id == element.Value.Id);

            if (existingElement?.Timestamp > element.Timestamp)
            {
                return this;
            }

            var adds = Adds.Where(e => e.Value.Id != element.Value.Id).ToList();
            adds.Add(element);

            Adds = adds.ToImmutableHashSet();

            return this;
        }

        public LWW_Set<T> Remove(LWW_SetElement<T> element)
        {
            var addedElement = Adds.FirstOrDefault(e => e.Value.Id == element.Value.Id);
            var existingElement = Removes.FirstOrDefault(e => e.Value.Id == element.Value.Id);

            if (addedElement is not null && addedElement?.Timestamp < element.Timestamp 
                                         && existingElement?.Timestamp < element.Timestamp)
            {
                var removes = Removes.Where(e => e.Value.Id != element.Value.Id).ToList();
                removes.Add(element);

                Removes = removes.ToImmutableHashSet();
            }

            return this;
        }

        public LWW_Set<T> Merge(LWW_Set<T> otherSet)
        {
            var adds = Adds.Union(otherSet.Adds);
            var removes = Removes.Union(otherSet.Removes);

            return new LWW_Set<T>(adds, removes);
        }
    }
}