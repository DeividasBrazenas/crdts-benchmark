using System;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;
using CRDT.Sets.Operations;

namespace CRDT.Sets.Commutative 
{
    public sealed class LWW_Set<T> : LWW_SetBase<T> where T : DistributedEntity
    {
        public LWW_Set()
        {
        }

        public LWW_Set(IImmutableSet<LWW_SetElement<T>> adds, IImmutableSet<LWW_SetElement<T>> removes) 
            : base(adds, removes)
        {
        }

        public LWW_Set<T> Add(LWW_SetOperation operation)
        {
            var value = operation.Value.ToObject<T>();

            if (value?.Id == Guid.Empty)
            {
                return this;
            }

            var existingElement = Adds.FirstOrDefault(e => Equals(e.Value, value));

            if (existingElement?.Timestamp > operation.Timestamp)
            {
                return this;
            }

            var adds = Adds.Where(e => !Equals(e.Value, value)).ToList();
            adds.Add(new LWW_SetElement<T>(value, operation.Timestamp));

            Adds = adds.ToImmutableHashSet();

            return this;
        }

        public LWW_Set<T> Remove(LWW_SetOperation operation)
        {
            var value = operation.Value.ToObject<T>();

            if (value?.Id == Guid.Empty)
            {
                return this;
            }

            var addedElement = Adds.FirstOrDefault(e => Equals(e.Value, value));
            var existingElement = Removes.FirstOrDefault(e => Equals(e.Value, value));

            if (addedElement is not null && 
                addedElement?.Timestamp < operation.Timestamp && 
                existingElement?.Timestamp < operation.Timestamp)
            {
                var removes = Removes.Where(e => !Equals(e.Value, value)).ToList();
                removes.Add(new LWW_SetElement<T>(value, operation.Timestamp));

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