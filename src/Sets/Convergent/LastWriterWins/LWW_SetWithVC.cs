using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Convergent.LastWriterWins
{
    public sealed class LWW_SetWithVC<T> : LWW_SetWithVCBase<T> where T : DistributedEntity
    {
        public LWW_SetWithVC()
        {
        }
        public LWW_SetWithVC(IImmutableSet<LWW_SetWithVCElement<T>> adds, IImmutableSet<LWW_SetWithVCElement<T>> removes)
            : base(adds, removes)
        {
        }

        public LWW_SetWithVC<T> Merge(IImmutableSet<LWW_SetWithVCElement<T>> adds, IImmutableSet<LWW_SetWithVCElement<T>> removes)
        {
            var addsUnion = Adds.Union(adds);
            var removesUnion = Removes.Union(removes);

            var filteredAdds = addsUnion
                .Where(a => !addsUnion.Any(oa => a.Value.Id == oa.Value.Id && a.VectorClock < oa.VectorClock));
            var filteredRemoves = removesUnion
                .Where(r => filteredAdds.Any(a => Equals(a.Value, r.Value)))
                .Where(a => !removesUnion.Any(oa => a.Value.Id == oa.Value.Id && a.VectorClock < oa.VectorClock));

            return new(filteredAdds.ToImmutableHashSet(), filteredRemoves.ToImmutableHashSet());
        }
    }
}