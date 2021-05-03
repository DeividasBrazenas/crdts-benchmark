using System;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Commutative.ObservedUpdatedRemoved
{
    public sealed class OUR_OptimizedSetWithVC<T> : OUR_OptimizedSetWithVCBase<T> where T : DistributedEntity
    {
        public OUR_OptimizedSetWithVC()
        {
        }

        public OUR_OptimizedSetWithVC(IImmutableSet<OUR_OptimizedSetWithVCElement<T>> elements)
            : base(elements)
        {
        }

        public OUR_OptimizedSetWithVC<T> Add(T value, Guid tag, VectorClock vectorClock)
        {
            var existingElement = Elements.FirstOrDefault(a => a.Value.Id == value.Id && a.Tag == tag);

            if (existingElement is not null)
            {
                return Update(value, tag, vectorClock);
            }

            return new(Elements.Add(new OUR_OptimizedSetWithVCElement<T>(value, tag, vectorClock, false)));
        }

        public OUR_OptimizedSetWithVC<T> Update(T value, Guid tag, VectorClock vectorClock)
        {
            var elementToUpdate = Elements.FirstOrDefault(a => a.Value.Id == value.Id && a.Tag == tag);

            if (elementToUpdate is null || elementToUpdate?.VectorClock > vectorClock)
            {
                return this;
            }

            var elements = Elements.Remove(elementToUpdate);
            elements = elements.Add(new OUR_OptimizedSetWithVCElement<T>(value, tag, vectorClock, false));

            return new(elements);
        }

        public OUR_OptimizedSetWithVC<T> Remove(T value, Guid tag, VectorClock vectorClock)
        {
            var elementToRemove = Elements.FirstOrDefault(a => Equals(a.Value, value) && a.Tag == tag);

            if (elementToRemove is null || elementToRemove?.VectorClock > vectorClock)
            {
                return this;
            }

            var elements = Elements.Remove(elementToRemove);

            return new(elements.Add(new OUR_OptimizedSetWithVCElement<T>(value, tag, vectorClock, true)));
        }
    }
}