using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Bases
{
    public abstract class LWW_SetWithVCBase<T> where T : DistributedEntity
    {
        public IImmutableSet<LWW_SetWithVCElement<T>> Adds { get; protected set; }

        public IImmutableSet<LWW_SetWithVCElement<T>> Removes { get; protected set; }

        protected LWW_SetWithVCBase()
        {
            Adds = ImmutableHashSet<LWW_SetWithVCElement<T>>.Empty;
            Removes = ImmutableHashSet<LWW_SetWithVCElement<T>>.Empty;
        }

        protected LWW_SetWithVCBase(IImmutableSet<LWW_SetWithVCElement<T>> adds, 
            IImmutableSet<LWW_SetWithVCElement<T>> removes)
        {
            Adds = adds;
            Removes = removes;
        }

        public bool Lookup(T value)
        {
            var added = Adds.FirstOrDefault(a => Equals(a.Value, value));
            var removed = Removes.FirstOrDefault(r => Equals(r.Value, value));

            if (added is not null && removed is null)
            {
                return true;
            }

            if (added is not null && added?.VectorClock > removed?.VectorClock)
            {
                return true;
            }

            return false;
        }
    }
}