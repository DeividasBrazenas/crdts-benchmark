﻿using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Convergent
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

        public LWW_Set<T> Add(LWW_SetElement<T> element)
        {
            var existingElement = Adds.FirstOrDefault(a => a.Value.Id == element.Value.Id);

            if (existingElement is not null)
            {
                return Update(element);
            }

            return new(Adds.Add(element), Removes);
        }

        public LWW_Set<T> Update(LWW_SetElement<T> element)
        {
            var elementToUpdate = Adds.FirstOrDefault(a => a.Value.Id == element.Value.Id);

            if (elementToUpdate is null || elementToUpdate?.Timestamp > element.Timestamp)
            {
                return this;
            }

            var adds = Adds.Remove(elementToUpdate);
            adds = adds.Add(element);

            return new(adds, Removes);
        }

        public LWW_Set<T> Remove(LWW_SetElement<T> element)
        {
            if (Adds.Any(a => Equals(a.Value, element.Value) && a.Timestamp < element.Timestamp))
            {
                return new(Adds, Removes.Add(element));
            }

            return this;
        }

        public LWW_Set<T> Merge(IImmutableSet<LWW_SetElement<T>> adds, IImmutableSet<LWW_SetElement<T>> removes)
        {
            var addsUnion = Adds.Union(adds);
            var removesUnion = Removes.Union(removes);

            var filteredAdds = addsUnion
                .Where(a => !addsUnion.Any(oa => a.Value.Id == oa.Value.Id && a.Timestamp < oa.Timestamp));
            var filteredRemoves = removesUnion
                .Where(r => filteredAdds.Any(a => Equals(a.Value, r.Value)))
                .Where(a => !removesUnion.Any(oa => a.Value.Id == oa.Value.Id && a.Timestamp < oa.Timestamp));

            return new(filteredAdds.ToImmutableHashSet(), filteredRemoves.ToImmutableHashSet());
        }
    }
}