using System;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Bases
{
    public abstract class LWW_SetBase<T> where T : DistributedEntity
    {
        public IImmutableSet<LWW_SetElement<T>> Adds { get; protected set; }

        public IImmutableSet<LWW_SetElement<T>> Removes { get; protected set; }

        protected LWW_SetBase()
        {
            Adds = ImmutableHashSet<LWW_SetElement<T>>.Empty;
            Removes = ImmutableHashSet<LWW_SetElement<T>>.Empty;
        }

        protected LWW_SetBase(IImmutableSet<LWW_SetElement<T>> adds, 
            IImmutableSet<LWW_SetElement<T>> removes)
        {
            Adds = adds;
            Removes = removes;
        }

        public bool Lookup(T value)
        {
            var added = Adds.Where(a => Equals(a.Value, value)).OrderBy(a => a.Timestamp).LastOrDefault();
            var removed = Removes.Where(r => Equals(r.Value, value)).OrderBy(r => r.Timestamp).LastOrDefault();

            if (added is not null && added?.Timestamp > removed?.Timestamp)
            {
                return true;
            }

            return false;
        }
    }
}