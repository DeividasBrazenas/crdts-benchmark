using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;

namespace CRDT.Sets.Convergent
{
    // 2P-Set
    public sealed class P_Set<T> : P_SetBase<T> where T : DistributedEntity
    {
        public P_Set()
        {
        }

        public P_Set(IImmutableSet<T> adds, IImmutableSet<T> removes) : base(adds, removes)
        {
        }

        public P_Set<T> Add(T value) => new(Adds.Add(value), Removes);

        public P_Set<T> Remove(T value)
        {
            if (Adds.Any(e => Equals(e, value)))
            {
                return new(Adds, Removes.Add(value));
            }

            return this;
        }

        public P_Set<T> Merge(IImmutableSet<T> adds, IImmutableSet<T> removes)
        {
            var addsUnion = Adds.Union(adds);

            var removesUnion = Removes.Union(removes);

            var validRemoves = removesUnion.Where(r => addsUnion.Any(a => Equals(a, r)));

            return new(addsUnion.ToImmutableHashSet(), validRemoves.ToImmutableHashSet());
        } 
    }
}