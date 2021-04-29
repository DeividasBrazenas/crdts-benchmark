using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;

namespace CRDT.Sets.Entities
{
    public class LWW_OptimizedSetElement<T> : ValueObject where T : DistributedEntity
    {
        public T Value { get; }

        public Timestamp Timestamp { get; }

        public bool Removed { get; }

        public LWW_OptimizedSetElement(T value, long timestamp, bool removed)
        {
            Value = value;
            Removed = removed;
            Timestamp = new Timestamp(timestamp);
        }

        public LWW_OptimizedSetElement(T value, Timestamp timestamp, bool removed)
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