using System;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Convergent
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

        public OUR_Set<T> Merge(IImmutableSet<OUR_SetElement<T>> adds, IImmutableSet<OUR_SetElement<T>> removes)
        {
            var addsUnion = Adds.Union(adds);
            var removesUnion = Removes.Union(removes);

            var filteredAdds = addsUnion
                .Where(a => !addsUnion.Any(oa => a.Value.Id == oa.Value.Id && a.Tag == oa.Tag && a.Timestamp < oa.Timestamp));
            var filteredRemoves = removesUnion
                .Where(r => filteredAdds.Any(a => Equals(a.Value, r.Value)))
                .Where(a => !removesUnion.Any(oa => a.Value.Id == oa.Value.Id && a.Tag == oa.Tag && a.Timestamp < oa.Timestamp));

            return new(filteredAdds.ToImmutableHashSet(), filteredRemoves.ToImmutableHashSet());
        }
    }
}