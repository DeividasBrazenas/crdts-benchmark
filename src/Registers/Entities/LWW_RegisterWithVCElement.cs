using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;

namespace CRDT.Registers.Entities
{
    public class LWW_RegisterWithVCElement<T> : ValueObject where T : DistributedEntity
    {
        public T Value { get; }

        public VectorClock VectorClock { get; }

        public bool Removed { get; }

        public LWW_RegisterWithVCElement(T value, VectorClock vectorClock, bool removed)
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