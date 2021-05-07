using System;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Commutative.ObservedUpdatedRemoved
{
    public sealed class OUR_OptimizedSet<T> : OUR_OptimizedSetBase<T> where T : DistributedEntity
    {
        public OUR_OptimizedSet()
        {
        }

        public OUR_OptimizedSet(ImmutableHashSet<OUR_OptimizedSetElement<T>> elements)
            : base(elements)
        {
        }

        public OUR_OptimizedSet<T> Add(T value, Guid tag, long timestamp)
        {
            var existingElement = Elements.FirstOrDefault(a => a.Value.Id == value.Id && a.Tag == tag);

            if (existingElement is not null)
            {
                return Update(value, tag, timestamp);
            }

            return new(Elements.Add(new OUR_OptimizedSetElement<T>(value, tag, timestamp, false)));
        }

        public OUR_OptimizedSet<T> Update(T value, Guid tag, long timestamp)
        {
            var elementToUpdate = Elements.FirstOrDefault(a => a.Value.Id == value.Id && a.Tag == tag);

            if (elementToUpdate is null || elementToUpdate?.Timestamp > timestamp)
            {
                return this;
            }

            var elements = Elements.Remove(elementToUpdate);
            elements = elements.Add(new OUR_OptimizedSetElement<T>(value, tag, timestamp, false));

            return new(elements);
        }

        public OUR_OptimizedSet<T> Remove(T value, Guid tag, long timestamp)
        {
            var elementToRemove = Elements.FirstOrDefault(a => Equals(a.Value, value) && a.Tag == tag);

            if (elementToRemove is null || elementToRemove?.Timestamp > timestamp)
            {
                return this;
            }

            var elements = Elements.Remove(elementToRemove);

            return new(elements.Add(new OUR_OptimizedSetElement<T>(value, tag, timestamp, true)));
        }
    }
}