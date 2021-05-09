using System;
using System.Collections.Generic;
using CRDT.Core.Abstractions;

namespace CRDT.Sets.Entities
{
    public class OUR_SetElement<T> : ValueObject where T : DistributedEntity
    {
        public Guid ValueId { get; }

        public T Value { get; }

        public Guid Tag { get; }

        public long Timestamp { get; }

        public OUR_SetElement(T value, Guid tag, long timestamp)
        {
            ValueId = value.Id;
            Value = value;
            Tag = tag;
            Timestamp = timestamp;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return ValueId;
            yield return Value;
            yield return Tag;
            yield return Timestamp;
        }

        public static bool operator <(OUR_SetElement<T> left, OUR_SetElement<T> right)
            => Compare(left, right) == 1;

        public static bool operator >(OUR_SetElement<T> left, OUR_SetElement<T> right)
            => Compare(left, right) == -1;

        private static int Compare(OUR_SetElement<T> left, OUR_SetElement<T> right)
        {
            if (left is null && right is null)
            {
                return 0;
            }

            if (left is null)
            {
                return 1;
            }

            if (right is null)
            {
                return -1;
            }

            if (left.Timestamp < right.Timestamp)
            {
                return 1;
            }

            if (left.Timestamp > right.Timestamp)
            {
                return -1;
            }

            if (left.Value.GetHashCode() < right.Value.GetHashCode())
            {
                return 1;
            }

            return -1;
        }
    }
}