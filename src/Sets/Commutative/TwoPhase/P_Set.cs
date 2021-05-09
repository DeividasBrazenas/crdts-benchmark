using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;

namespace CRDT.Sets.Commutative.TwoPhase 
{
    // 2P-Set
    public sealed class P_Set<T> : P_SetBase<T> where T : DistributedEntity
    {
        public P_Set()
        {
        }

        public P_Set(ImmutableHashSet<T> adds, ImmutableHashSet<T> removes) : base(adds, removes)
        {
        }

        public P_Set<T> Add(T value)
        {
            var add = Adds.FirstOrDefault(e => e.Id == value.Id);
            var remove = Removes.FirstOrDefault(e => e.Id == value.Id);

            if (add is not null || remove is not null)
            {
                return this;
            }

            return new(Adds.Add(value), Removes);
        }

        public P_Set<T> Remove(T value)
        {
            if (Adds.Any(e => e.Id == value.Id))
            {
                return new(Adds, Removes.Add(value));
            }

            return this;
        }
    }
}