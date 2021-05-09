using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Commutative.LastWriterWins
{
    public sealed class LWW_OptimizedSetWithVC<T> : LWW_OptimizedSetWithVCBase<T> where T : DistributedEntity
    {
        public LWW_OptimizedSetWithVC()
        {
        }

        public LWW_OptimizedSetWithVC(ImmutableHashSet<LWW_OptimizedSetWithVCElement<T>> elements)
            : base(elements)
        {
        }

        public LWW_OptimizedSetWithVC<T> Assign(T value, VectorClock vectorClock)
        {
            var existingElement = Elements.FirstOrDefault(a => a.Value.Id == value.Id);

            if (existingElement is not null && existingElement.VectorClock < vectorClock)
            {
                var elements = Elements.Remove(existingElement);
                
                return new(elements.Add(new LWW_OptimizedSetWithVCElement<T>(value, vectorClock, false)));
            }

            if (existingElement is null)
            {
                return new(Elements.Add(new LWW_OptimizedSetWithVCElement<T>(value, vectorClock, false)));
            }

            return this;
        }

        public LWW_OptimizedSetWithVC<T> Remove(T value, VectorClock vectorClock)
        {
            var add = Elements.FirstOrDefault(e => Equals(e.Value, value));

            if (add is not null && add.VectorClock < vectorClock)
            {
                var elements = Elements.Remove(add);

                return new(elements.Add(new LWW_OptimizedSetWithVCElement<T>(value, vectorClock, true)));
            }

            return this;
        }
    }
}