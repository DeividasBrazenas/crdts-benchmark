using System;
using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;

namespace CRDT.Sets.Entities
{
    public class OUR_OptimizedSetElement<T> : ValueObject where T : DistributedEntity
    {
        public T Value { get; }

        public Guid Tag { get; }

        public long Timestamp { get; }

        public bool Removed { get; }

        public OUR_OptimizedSetElement(T value, Guid tag, long timestamp, bool removed)
        {
            Value = value;
            Tag = tag;
            Timestamp = timestamp;
            Removed = removed;
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return Tag;
            yield return Timestamp;
            yield return Removed;
        }

        public static bool operator <(OUR_OptimizedSetElement<T> left, OUR_OptimizedSetElement<T> right)
            => Compare(left, right) == 1;

        public static bool operator >(OUR_OptimizedSetElement<T> left, OUR_OptimizedSetElement<T> right)
            => Compare(left, right) == -1;

        private static int Compare(OUR_OptimizedSetElement<T> left, OUR_OptimizedSetElement<T> right)
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