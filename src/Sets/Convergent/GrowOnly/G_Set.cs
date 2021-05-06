using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;

namespace CRDT.Sets.Convergent.GrowOnly
{
    public sealed class G_Set<T> : G_SetBase<T> where T : DistributedEntity
    {
        public G_Set()
        {
        }

        public G_Set(ImmutableHashSet<T> values) : base(values)
        {
        }

        public G_Set<T> Add(T value) => new(Values.Add(value));

        public G_Set<T> Merge(ImmutableHashSet<T> values)
        {
            return new(Values.Union(values));
        }
    }
}