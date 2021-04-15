using System;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;

namespace CRDT.Sets.Commutative 
{
    public sealed class P_Set<T> : P_SetBase<T> where T : DistributedEntity
    {
        public P_Set(IImmutableSet<T> adds, IImmutableSet<T> removes) : base(adds, removes)
        {
        }

        public P_Set<T> Add(Operation operation)
        {
            var value = operation.Value.ToObject<T>();

            Adds = Adds.Add(value);

            return this;
        }

        public P_Set<T> Remove(Operation operation)
        {
            var value = operation.Value.ToObject<T>();

            Removes = Removes.Add(value);

            return this;
        }

        public T Value(Guid id)
        {
            if (Removes.Any(e => e.Id == id))
            {
                return null;
            }

            return Adds.FirstOrDefault(e => e.Id == id);
        }

        public P_Set<T> Merge(P_Set<T> otherSet)
        {
            var adds = Adds.Union(otherSet.Adds);
            var removes = Removes.Union(otherSet.Removes);

            return new P_Set<T>(adds, removes);
        }
    }
}