using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Convergent
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

        public LWW_Set<T> Add(LWW_SetElement<T> element)
        {
            if (!Adds.Contains(element))
            {
                Adds = Adds.Add(element);
            }

            return this;
        }

        public LWW_Set<T> Remove(LWW_SetElement<T> element)
        {
            var addedElement = Adds.FirstOrDefault(e => Equals(e.Value, element.Value));

            if (addedElement is not null)
            {
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