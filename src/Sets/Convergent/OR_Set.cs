using System;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Convergent
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

        public OR_Set<T> Add(OR_SetElement<T> element)
        {
            Adds = Adds.Add(element);

            return this;
        }

        public OR_Set<T> Remove(OR_SetElement<T> element)
        {
            if (Adds.Any(e => e.Tag == element.Tag && Equals(e.Value, element.Value)))
            {
                Removes = Removes.Add(element);
            }

            return this;
        }

        public IImmutableSet<T> Values =>
                Adds
                .Where(a => !Removes.Any(r => r.Tag == a.Tag && Equals(r.Value, a.Value)))
                .Select(e => e.Value)
                .Distinct()
                .ToImmutableHashSet();

        public T Value(Guid id)
        {
            return Values.FirstOrDefault(v => v.Id == id);
        }

        public OR_Set<T> Merge(OR_Set<T> otherSet)
        {
            var adds = Adds.Union(otherSet.Adds);
            var removes = Removes.Union(otherSet.Removes);

            return new OR_Set<T>(adds, removes);
        }
    }
}