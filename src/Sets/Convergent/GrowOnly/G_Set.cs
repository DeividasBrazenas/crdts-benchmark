using System.Collections.Immutable;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;

namespace CRDT.Sets.Convergent.GrowOnly
{
    public sealed class G_Set<T> : G_SetBase<T> where T : DistributedEntity
    {
        public G_Set()
        {
        }

        public G_Set(IImmutableSet<T> values) : base(values)
        {
        }

        public G_Set<T> Add(T value) => new(Values.Add(value));

        public G_Set<T> Merge(IImmutableSet<T> values) => new(Values.Union(values));
    }
}