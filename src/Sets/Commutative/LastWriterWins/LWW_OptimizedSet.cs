﻿using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Commutative.LastWriterWins
{
    public sealed class LWW_OptimizedSet<T> : LWW_OptimizedSetBase<T> where T : DistributedEntity
    {
        public LWW_OptimizedSet()
        {
        }

        public LWW_OptimizedSet(IImmutableSet<LWW_OptimizedSetElement<T>> elements)
            : base(elements)
        {
        }

        public LWW_OptimizedSet<T> Add(T value, long timestamp)
        {
            var existingElement = Elements.FirstOrDefault(a => a.Value.Id == value.Id);

            if (existingElement is not null && existingElement.Timestamp < new Timestamp(timestamp))
            {
                var elements = Elements.Remove(existingElement);
                
                return new(elements.Add(new LWW_OptimizedSetElement<T>(value, timestamp, false)));
            }

            if (existingElement is null)
            {
                return new(Elements.Add(new LWW_OptimizedSetElement<T>(value, timestamp, false)));
            }

            return this;
        }

        public LWW_OptimizedSet<T> Remove(T value, long timestamp)
        {
            var add = Elements.FirstOrDefault(e => Equals(e.Value, value));

            if (add is not null && add.Timestamp < new Timestamp(timestamp))
            {
                var elements = Elements.Remove(add);

                return new(elements.Add(new LWW_OptimizedSetElement<T>(value, timestamp, true)));
            }

            return this;
        }
    }
}