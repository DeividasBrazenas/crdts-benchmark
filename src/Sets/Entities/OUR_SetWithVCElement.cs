using System;
using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;

namespace CRDT.Sets.Entities
{
    public class OUR_SetWithVCElement<T> : ValueObject where T : DistributedEntity
    {
        public T Value { get; }

        public Guid Tag { get; }

        public VectorClock VectorClock { get; }

        public OUR_SetWithVCElement(T value, Guid tag, VectorClock timestamp)
        {
            Value = value;
            Tag = tag;
            VectorClock = timestamp;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return Tag;
            yield return VectorClock;
        }

        public static bool operator <(OUR_SetWithVCElement<T> left, OUR_SetWithVCElement<T> right)
            => Compare(left, right) == 1;

        public static bool operator >(OUR_SetWithVCElement<T> left, OUR_SetWithVCElement<T> right)
            => Compare(left, right) == -1;

        private static int Compare(OUR_SetWithVCElement<T> left, OUR_SetWithVCElement<T> right)
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