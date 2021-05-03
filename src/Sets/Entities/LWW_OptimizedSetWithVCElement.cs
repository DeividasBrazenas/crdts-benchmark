using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;

namespace CRDT.Sets.Entities
{
    public class LWW_OptimizedSetWithVCElement<T> : ValueObject where T : DistributedEntity
    {
        public T Value { get; }

        public VectorClock VectorClock { get; }

        public bool Removed { get; }

        public LWW_OptimizedSetWithVCElement(T value, VectorClock vectorClock, bool removed)
        {
            Value = value;
            VectorClock = vectorClock;
            Removed = removed;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return VectorClock;
            yield return Removed;
        }
    }
}