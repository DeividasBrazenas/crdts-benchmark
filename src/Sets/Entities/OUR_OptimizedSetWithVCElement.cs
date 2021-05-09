using System;
using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;

namespace CRDT.Sets.Entities
{
    public class OUR_OptimizedSetWithVCElement<T> : ValueObject where T : DistributedEntity
    {
        public Guid ValueId { get; }

        public T Value { get; }

        public Guid Tag { get; }

        public VectorClock VectorClock { get; }

        public bool Removed { get; }

        public OUR_OptimizedSetWithVCElement(T value, Guid tag, VectorClock vectorClock, bool removed)
        {
            ValueId = value.Id;
            Value = value;
            Tag = tag;
            VectorClock = vectorClock;
            Removed = removed;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return ValueId;
            yield return Value;
            yield return Tag;
            yield return VectorClock;
            yield return Removed;
        }

        public static bool operator <(OUR_OptimizedSetWithVCElement<T> left, OUR_OptimizedSetWithVCElement<T> right)
            => Compare(left, right) == 1;

        public static bool operator >(OUR_OptimizedSetWithVCElement<T> left, OUR_OptimizedSetWithVCElement<T> right)
            => Compare(left, right) == -1;

        private static int Compare(OUR_OptimizedSetWithVCElement<T> left, OUR_OptimizedSetWithVCElement<T> right)
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

            if (left.VectorClock < right.VectorClock)
            {
                return 1;
            }

            if (left.VectorClock > right.VectorClock)
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