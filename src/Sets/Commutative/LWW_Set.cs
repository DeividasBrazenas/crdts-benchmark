using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;
using CRDT.Sets.Operations;

namespace CRDT.Sets.Commutative 
{
    public sealed class LWW_Set<T> : LWW_SetBase<T> where T : DistributedEntity
    {
        public LWW_Set()
        {
        }

        public LWW_Set(IImmutableSet<LWW_SetElement<T>> adds, IImmutableSet<LWW_SetElement<T>> removes) 
            : base(adds, removes)
        {
        }

        public LWW_Set<T> Add(LWW_SetOperation operation)
        {
            var value = operation.Value.ToObject<T>();

            var element = new LWW_SetElement<T>(value, operation.Timestamp);

            if (!Adds.Contains(element))
            {
                Adds = Adds.Add(element);
            }

            return this;
        }

        public LWW_Set<T> Remove(LWW_SetOperation operation)
        {
            var value = operation.Value.ToObject<T>();

            var addedElement = Adds.FirstOrDefault(e => Equals(e.Value, value));

            if (addedElement is not null)
            {
                var element = new LWW_SetElement<T>(value, operation.Timestamp);

                if (!Removes.Contains(element))
                {
                    Removes = Removes.Add(element);
                }
            }

            return this;
        }

        public LWW_Set<T> Merge(LWW_Set<T> otherSet)
        {
            var adds = Adds.Union(otherSet.Adds);
            var removes = Removes.Union(otherSet.Removes);

            return new LWW_Set<T>(adds, removes);
        }
    }
}