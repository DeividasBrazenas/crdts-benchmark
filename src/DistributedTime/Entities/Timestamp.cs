using System;

namespace CRDT.DistributedTime.Entities
{
    public sealed class Timestamp
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
            => left.Value < right.Value;

        public static bool operator >(Timestamp left, Timestamp right)
            => left.Value > right.Value;
    }
}