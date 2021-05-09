using System.Collections.Generic;
using CRDT.Core.Abstractions;

namespace CRDT.Sets.Entities
{
    public class P_OptimizedSetElement<T> : ValueObject where T : DistributedEntity
    {
        public T Value { get; }

        public bool Removed { get; }

        public P_OptimizedSetElement(T value, bool removed)
        {
            Value = value;
            Removed = removed;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return Removed;
        }
    }
}