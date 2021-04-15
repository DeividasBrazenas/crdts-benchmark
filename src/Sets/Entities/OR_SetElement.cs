using System;
using System.Collections.Generic;
using CRDT.Core.Abstractions;

namespace CRDT.Sets.Entities
{
    public class OR_SetElement<T> : ValueObject where T : DistributedEntity
    {
        public T Value { get; }

        public Guid Tag { get; }

        public OR_SetElement(T value, Guid tag)
        {
            Value = value;
            Tag = tag;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return Tag;
        }
    }
}