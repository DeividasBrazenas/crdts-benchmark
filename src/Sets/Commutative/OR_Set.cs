using System;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;
using CRDT.Sets.Operations;

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

        public OR_Set<T> Add(OR_SetOperation operation)
        {
            var value = operation.Value.ToObject<T>();
            var element = new OR_SetElement<T>(value, operation.Tag);

            Adds = Adds.Add(element);

            return this;
        }

        public OR_Set<T> Remove(OR_SetOperation operation)
        {
            var value = operation.Value.ToObject<T>();

            if (Adds.Any(e => e.Tag == operation.Tag && Equals(e.Value, value)))
            {
                var element = new OR_SetElement<T>(value, operation.Tag);

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