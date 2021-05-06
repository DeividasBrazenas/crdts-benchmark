using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Convergent.ObservedUpdatedRemoved
{
    public sealed class OUR_SetWithVC<T> : OUR_SetWithVCBase<T> where T : DistributedEntity
    {
        public OUR_SetWithVC()
        {
        }

        public OUR_SetWithVC(ImmutableHashSet<OUR_SetWithVCElement<T>> adds, ImmutableHashSet<OUR_SetWithVCElement<T>> removes)
            : base(adds, removes)
        {
        }

        public OUR_SetWithVC<T> Merge(ImmutableHashSet<OUR_SetWithVCElement<T>> adds, ImmutableHashSet<OUR_SetWithVCElement<T>> removes)
        {
            var addsUnion = Adds.Union(adds);
            var removesUnion = Removes.Union(removes);

            var filteredAdds = addsUnion
                .Where(a => !addsUnion.Any(oa => a.Value.Id == oa.Value.Id && a.Tag == oa.Tag && a.VectorClock < oa.VectorClock));
            var filteredRemoves = removesUnion
                .Where(r => filteredAdds.Any(a => Equals(a.Value, r.Value)))
                .Where(a => !removesUnion.Any(oa => a.Value.Id == oa.Value.Id && a.Tag == oa.Tag && a.VectorClock < oa.VectorClock));

            return new(filteredAdds.ToImmutableHashSet(), filteredRemoves.ToImmutableHashSet());
        }
    }
}