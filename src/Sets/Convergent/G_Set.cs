using System.Collections.Immutable;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;

namespace CRDT.Sets.Convergent
{
    public sealed class G_Set<T> : G_SetBase<T> where T : DistributedEntity
    {
        public G_Set()
        {
        }

        public G_Set(IImmutableSet<T> values) : base(values)
        {
            Values = values;
        }

        public G_Set<T> Add(T value)
        {
            Values = Values.Add(value);

            return this;
        }

        public G_Set<T> Merge(G_Set<T> otherSet)
        {
            var mergedElements = Values.Union(otherSet.Values);

            return new G_Set<T>(mergedElements);
        }
    }
}