using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Convergent.LastWriterWins
{
    public sealed class LWW_Set<T> : LWW_SetBase<T> where T : DistributedEntity
    {
        public LWW_Set()
        {
        }
        public LWW_Set(ImmutableHashSet<LWW_SetElement<T>> adds, ImmutableHashSet<LWW_SetElement<T>> removes)
            : base(adds, removes)
        {
        }

        public LWW_Set<T> Add(T value, long timestamp)
        {
            var existingElement = Adds.FirstOrDefault(a => a.Value.Id == value.Id);

            if (existingElement is not null && existingElement.Timestamp < new Timestamp(timestamp))
            {
                var elements = Adds.Remove(existingElement);

                return new(elements.Add(new LWW_SetElement<T>(value, timestamp)), Removes);
            }

            if (existingElement is null)
            {
                return new(Adds.Add(new LWW_SetElement<T>(value, timestamp)), Removes);
            }

            return this;
        }

        public LWW_Set<T> Remove(T value, long timestamp)
        {
            if (Adds.Any(a => Equals(a.Value, value) && a.Timestamp < new Timestamp(timestamp)))
            {
                var element = Removes.FirstOrDefault(r => r.Value.Id == value.Id);

                ImmutableHashSet<LWW_SetElement<T>> elements = Removes;

                if (element is not null)
                {
                    elements = Removes.Remove(element);
                }

                return new(Adds, elements.Add(new LWW_SetElement<T>(value, timestamp)));
            }

            return this;
        }
        public LWW_Set<T> Merge(ImmutableHashSet<LWW_SetElement<T>> adds, ImmutableHashSet<LWW_SetElement<T>> removes)
        {
            return new(Adds.Union(adds), Removes.Union(removes));
        }
    }
}