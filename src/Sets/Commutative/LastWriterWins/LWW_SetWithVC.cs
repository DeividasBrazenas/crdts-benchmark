using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Commutative.LastWriterWins 
{
    public sealed class LWW_SetWithVC<T> : LWW_SetWithVCBase<T> where T : DistributedEntity
    {
        public LWW_SetWithVC()
        {
        }

        public LWW_SetWithVC(ImmutableHashSet<LWW_SetWithVCElement<T>> adds, ImmutableHashSet<LWW_SetWithVCElement<T>> removes) 
            : base(adds, removes)
        {
        }

        public LWW_SetWithVC<T> Add(T value, VectorClock vectorClock)
        {
            var existingElement = Adds.FirstOrDefault(a => a.Value.Id == value.Id);

            if (existingElement is not null && existingElement.VectorClock < vectorClock)
            {
                var elements = Adds.Remove(existingElement);

                return new(elements.Add(new LWW_SetWithVCElement<T>(value, vectorClock)), Removes);
            }

            if (existingElement is null)
            {
                return new(Adds.Add(new LWW_SetWithVCElement<T>(value, vectorClock)), Removes);
            }

            return this;
        }

        public LWW_SetWithVC<T> Remove(T value, VectorClock vectorClock)
        {
            if (Adds.Any(a => Equals(a.Value, value) && a.VectorClock < vectorClock))
            {
                var element = Removes.FirstOrDefault(r => r.Value.Id == value.Id);

                ImmutableHashSet<LWW_SetWithVCElement<T>> elements = Removes;

                if (element is not null)
                {
                    elements = Removes.Remove(element);
                }

                return new(Adds, elements.Add(new LWW_SetWithVCElement<T>(value, vectorClock)));
            }

            return this;
        }
    }
}