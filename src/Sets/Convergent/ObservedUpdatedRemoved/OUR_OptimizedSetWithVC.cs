using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Convergent.ObservedUpdatedRemoved
{
    public sealed class OUR_OptimizedSetWithVC<T> : OUR_OptimizedSetWithVCBase<T> where T : DistributedEntity
    {
        public OUR_OptimizedSetWithVC()
        {
        }

        public OUR_OptimizedSetWithVC(IImmutableSet<OUR_OptimizedSetWithVCElement<T>> elements)
            : base(elements)
        {
        }

        public OUR_OptimizedSetWithVC<T> Merge(IImmutableSet<OUR_OptimizedSetWithVCElement<T>> elements)
        {
            var union = Elements.Union(elements);

            var filteredElements =
                union.Where(ue => !union.Any(e => Equals(ue.Value.Id, e.Value.Id) && ue.Tag == e.Tag && ue.VectorClock < e.VectorClock));

            return new(filteredElements.ToImmutableHashSet());
        }
    }
}