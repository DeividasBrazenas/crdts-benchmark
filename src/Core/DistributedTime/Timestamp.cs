using System;
using System.Collections.Generic;
using CRDT.Core.Abstractions;

namespace CRDT.Core.DistributedTime
{
    public sealed class Timestamp : ValueObject
    {
        public long Value { get; }

        public Timestamp()
        {
            Value = DateTime.Now.Ticks + 1;
        }

        public Timestamp(long timestamp)
        {
            Value = timestamp;
        }

        public static bool operator <(Timestamp left, Timestamp right)
            => Compare(left, right) == 1;

        public static bool operator >(Timestamp left, Timestamp right)
            => Compare(left, right) == -1;

        public static int Compare(Timestamp left, Timestamp right)
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

            if (left.Value > right.Value)
            {
                return -1;
            }

            if (left.Value < right.Value)
            {
                return 1;
            }

            return 0;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}