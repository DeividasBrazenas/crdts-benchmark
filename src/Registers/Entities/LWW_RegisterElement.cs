using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;

namespace CRDT.Registers.Entities
{
    public class LWW_RegisterElement<T> : ValueObject where T : DistributedEntity
    {
        public T Value { get; }

        public Timestamp Timestamp { get; set; }

        public LWW_RegisterElement(T value, long timestamp)
        {
            Value = value;
            Timestamp = new Timestamp(timestamp);
        }

        public LWW_RegisterElement(T value, Timestamp timestamp)
        {
            Value = value;
            Timestamp = timestamp;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return Timestamp;
        }
    }
}