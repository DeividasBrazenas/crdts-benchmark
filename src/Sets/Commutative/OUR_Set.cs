using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;
using CRDT.Sets.Operations;

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

        public OUR_Set<T> Add(OUR_SetOperation operation)
        {
            var value = operation.Value.ToObject<T>();

            var element = new OUR_SetElement<T>(value, operation.Timestamp);

            var addConflicts = new HashSet<OUR_SetElement<T>> { element };
            var removeConflicts = new HashSet<OUR_SetElement<T>>();

            var adds = new HashSet<OUR_SetElement<T>>();
            var removes = new HashSet<OUR_SetElement<T>>();

            foreach (var add in Adds)
            {
                if (element.Value.Id == add.Value.Id)
                {
                    addConflicts.Add(add);
                }
                else
                {
                    adds.Add(add);
                }
            }

            foreach (var remove in Removes)
            {
                if (element.Value.Id == remove.Value.Id)
                {
                    removeConflicts.Add(remove);
                }
                else
                {
                    removes.Add(remove);
                }
            }

            var addWinner = addConflicts.OrderBy(e => e.Timestamp).Last();
            var removeWinner = removeConflicts.OrderBy(e => e.Timestamp).LastOrDefault();

            if (addWinner < removeWinner)
            {
                removes.Add(removeWinner);
            }

            adds.Add(addWinner);

            Adds = adds.ToImmutableHashSet();
            Removes = removes.ToImmutableHashSet();

            return this;
        }

        public OUR_Set<T> Remove(OUR_SetOperation operation)
        {
            var value = operation.Value.ToObject<T>();

            if (Adds.Any(e => Equals(value, e.Value)))
            {
                var element = new OUR_SetElement<T>(value, operation.Timestamp);

                Removes = Removes.Add(element);
            }

            return this;
        }

        public IImmutableSet<T> Values =>
            Adds
                .Where(a => Removes.All(r => a.Value.Id != r.Value.Id))
                .Select(e => e.Value)
                .Distinct()
                .ToImmutableHashSet();

        public T Value(Guid id)
        {
            return Values.FirstOrDefault(v => v.Id == id);
        }

        public OUR_Set<T> Merge(OUR_Set<T> otherSet)
        {
            var adds = Adds.Union(otherSet.Adds);
            var removes = Removes.Union(otherSet.Removes);

            return new OUR_Set<T>(adds, removes);
        }
    }
}