using System.Collections.Immutable;
using CRDT.Abstractions.Entities;
using CRDT.Sets.Bases;

namespace CRDT.Sets.Commutative 
{
    public sealed class G_Set<T> : G_SetBase<T> where T : DistributedEntity
    {
        public G_Set(IImmutableSet<T> values) : base(values)
        {
            Values = values;
        }

        public G_Set<T> Add(Operation operation)
        {
            var value = operation.Value.ToObject<T>();

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