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

        public LWW_Set<T> Add(LWW_SetElement<T> element) => new(Adds.Add(element), Removes);

        public LWW_Set<T> Remove(LWW_SetElement<T> element)
        {
            if (Adds.Any(a => Equals(a.Value, element.Value)))
            {
                return new(Adds, Removes.Add(element));
            }

            return this;
        }
    }
}