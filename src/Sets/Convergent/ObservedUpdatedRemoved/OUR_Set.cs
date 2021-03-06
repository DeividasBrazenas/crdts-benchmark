using System;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Convergent.ObservedUpdatedRemoved
{
    public sealed class OUR_Set<T> : OUR_SetBase<T> where T : DistributedEntity
    {
        public OUR_Set()
        {
        }

        public OUR_Set(ImmutableHashSet<OUR_SetElement<T>> adds, ImmutableHashSet<OUR_SetElement<T>> removes)
            : base(adds, removes)
        {
        }

        public OUR_Set<T> Add(T value, Guid tag, long timestamp)
        {
            var existingElement = Adds.FirstOrDefault(a => a.Value.Id == value.Id && a.Tag == tag);

            if (existingElement is not null)
            {
                return Update(value, tag, timestamp);
            }

            return new(Adds.Add(new OUR_SetElement<T>(value, tag, timestamp)), Removes);
        }

        public OUR_Set<T> Update(T value, Guid tag, long timestamp)
        {
            var elementToUpdate = Adds.FirstOrDefault(a => a.Value.Id == value.Id && a.Tag == tag);

            if (elementToUpdate is null || elementToUpdate?.Timestamp > timestamp)
            {
                return this;
            }

            var adds = Adds.Remove(elementToUpdate);
            adds = adds.Add(new OUR_SetElement<T>(value, tag, timestamp));

            return new(adds, Removes);
        }

        public OUR_Set<T> Remove(T value, Guid tag, long timestamp)
        {
            var elementToRemove = Adds.FirstOrDefault(a => Equals(a.Value, value) && a.Tag == tag);

            if (elementToRemove is null || elementToRemove?.Timestamp > timestamp)
            {
                return this;
            }

            return new(Adds, Removes.Add(new OUR_SetElement<T>(value, tag, timestamp)));
        }
        public OUR_Set<T> Merge(ImmutableHashSet<OUR_SetElement<T>> adds, ImmutableHashSet<OUR_SetElement<T>> removes)
        {
            return new(Adds.Union(adds), Removes.Union(removes));
        }
    }
}