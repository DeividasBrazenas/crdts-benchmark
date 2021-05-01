using System;
using System.Collections.Generic;
using CRDT.Core.Abstractions;

namespace CRDT.Sets.Entities
{
    public class OR_OptimizedSetElement<T> : ValueObject where T : DistributedEntity
    {
        public T Value { get; }

        public Guid Tag { get; }

        public bool Removed { get; }

        public OR_OptimizedSetElement(T value, Guid tag, bool removed)
        {
            Value = value;
            Tag = tag;
            Removed = removed;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return Tag;
            yield return Removed;
        }
    }
}