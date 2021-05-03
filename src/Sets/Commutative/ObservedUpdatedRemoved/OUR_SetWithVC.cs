using System;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Commutative.ObservedUpdatedRemoved
{
    public sealed class OUR_SetWithVC<T> : OUR_SetWithVCBase<T> where T : DistributedEntity
    {
        public OUR_SetWithVC()
        {
        }

        public OUR_SetWithVC(IImmutableSet<OUR_SetWithVCElement<T>> adds, IImmutableSet<OUR_SetWithVCElement<T>> removes)
            : base(adds, removes)
        {
        }

        public OUR_SetWithVC<T> Add(T value, Guid tag, VectorClock vectorClock)
        {
            var existingElement = Adds.FirstOrDefault(a => a.Value.Id == value.Id && a.Tag == tag);

            if (existingElement is not null)
            {
                return Update(value, tag, vectorClock);
            }

            return new(Adds.Add(new OUR_SetWithVCElement<T>(value, tag, vectorClock)), Removes);
        }

        public OUR_SetWithVC<T> Update(T value, Guid tag, VectorClock vectorClock)
        {
            var elementToUpdate = Adds.FirstOrDefault(a => a.Value.Id == value.Id && a.Tag == tag);

            if (elementToUpdate is null || elementToUpdate?.VectorClock > vectorClock)
            {
                return this;
            }

            var adds = Adds.Remove(elementToUpdate);
            adds = adds.Add(new OUR_SetWithVCElement<T>(value, tag, vectorClock));

            return new(adds, Removes);
        }

        public OUR_SetWithVC<T> Remove(T value, Guid tag, VectorClock vectorClock)
        {
            var elementToRemove = Adds.FirstOrDefault(a => Equals(a.Value, value) && a.Tag == tag);

            if (elementToRemove is null || elementToRemove?.VectorClock > vectorClock)
            {
                return this;
            }

            return new(Adds, Removes.Add(new OUR_SetWithVCElement<T>(value, tag, vectorClock)));
        }
    }
}