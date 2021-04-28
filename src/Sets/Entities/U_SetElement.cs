using System.Collections.Generic;
using CRDT.Core.Abstractions;

namespace CRDT.Sets.Entities
{
    public class U_SetElement<T> : ValueObject where T : DistributedEntity
    {
        public T Value { get; }

        public bool Removed { get; }

        public U_SetElement(T value, bool removed)
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