﻿using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;

namespace CRDT.Registers.Convergent
{
    public sealed class LWW_Register<T> : Bases.LWW_RegisterBase<T> where T : DistributedEntity
    {
        public Timestamp Timestamp { get; }

        public LWW_Register(T value, Node updatedBy) : base(value, updatedBy)
        {
            Timestamp = new Timestamp();
        }

        public LWW_Register(T value, Node updatedBy, long timestamp) : base(value, updatedBy)
        {
            Timestamp = new Timestamp(timestamp);
        }

        public LWW_Register<T> Merge(LWW_Register<T> other)
        {
            if (Timestamp > other.Timestamp)
            {
                return this;
            }

            if (Timestamp < other.Timestamp)
            {
                return other;
            }

            if (UpdatedBy < other.UpdatedBy)
            {
                return this;
            }

            return other;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return Timestamp;
            yield return UpdatedBy;
        }
    }
}