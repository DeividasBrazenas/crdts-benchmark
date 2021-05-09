using System;
using System.Collections.Generic;
using CRDT.Core.Abstractions;

namespace CRDT.Sets.Entities
{
    public class OR_OptimizedSetElement<T> : ValueObject where T : DistributedEntity
    {
        public Guid ValueId { get; }

        public T Value { get; }

        public Guid Tag { get; }

        public bool Removed { get; }

        public OR_OptimizedSetElement(T value, Guid tag, bool removed)
        {
            ValueId = value.Id;
            Value = value;
            Tag = tag;
            Removed = removed;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return ValueId;
            yield return Value;
            yield return Tag;
            yield return Removed;
        }
    }
}