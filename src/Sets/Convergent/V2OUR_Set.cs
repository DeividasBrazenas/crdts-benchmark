using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Convergent
{
    public sealed class V2OUR_Set<T> : V2OUR_SetBase<T> where T : DistributedEntity
    {
        public V2OUR_Set()
        {
        }

        public V2OUR_Set(IImmutableSet<OUR_SetElement<T>> adds, IImmutableSet<OUR_SetElement<T>> removes)
            : base(adds, removes)
        {
        }

        public V2OUR_Set<T> Add(OUR_SetElement<T> element)
        {
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

        public V2OUR_Set<T> Remove(OUR_SetElement<T> element)
        {
            if (Adds.Any(e => element.Value == e.Value))
            {
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

        public V2OUR_Set<T> Merge(V2OUR_Set<T> otherSet)
        {
            var adds = Adds.Union(otherSet.Adds);
            var removes = Removes.Union(otherSet.Removes);

            return new V2OUR_Set<T>(adds, removes);
        }
    }
}