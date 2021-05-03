using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;

namespace CRDT.Sets.Entities
{
    public class LWW_SetWithVCElement<T> : ValueObject where T : DistributedEntity
    {
        public T Value { get; }

        public VectorClock VectorClock { get; set; }

        public LWW_SetWithVCElement(T value, VectorClock vectorClock)
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