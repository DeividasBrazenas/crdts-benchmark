using System.Collections.Generic;
using CRDT.Core.Abstractions;

namespace CRDT.Registers.Entities
{
    public class LWW_RegisterElement<T> : ValueObject where T : DistributedEntity
    {
        public T Value { get; }

        public long Timestamp { get; }

        public bool Removed { get; }


        public LWW_RegisterElement(T value, long timestamp, bool removed)
        {
            Value = value;
            Timestamp = timestamp;
            Removed = removed;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return Timestamp;
            yield return Removed;
        }
    }
}