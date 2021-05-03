using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;

namespace CRDT.Registers.Entities
{
    public class LWW_RegisterWithVCElement<T> : ValueObject where T : DistributedEntity
    {
        public T Value { get; }

        public VectorClock VectorClock { get; }

        public LWW_RegisterWithVCElement(T value, VectorClock vectorClock)
        {
            Value = value;
            VectorClock = vectorClock;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return VectorClock;
        }
    }
}