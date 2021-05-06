using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Convergent.ObservedUpdatedRemoved
{
    public sealed class OUR_OptimizedSet<T> : OUR_OptimizedSetBase<T> where T : DistributedEntity
    {
        public OUR_OptimizedSet()
        {
        }

        public OUR_OptimizedSet(ImmutableHashSet<OUR_OptimizedSetElement<T>> elements)
            : base(elements)
        {
        }

        public OUR_OptimizedSet<T> Merge(ImmutableHashSet<OUR_OptimizedSetElement<T>> elements)
        {
            var union = Elements.Union(elements);

            var filteredElements =
                union.Where(ue => !union.Any(e => Equals(ue.Value.Id, e.Value.Id) && ue.Tag == e.Tag && ue.Timestamp < e.Timestamp));

            return new(filteredElements.ToImmutableHashSet());
        }
    }
}