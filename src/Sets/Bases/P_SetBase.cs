using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;

namespace CRDT.Sets.Bases
{
    public abstract class P_SetBase<T> where T : DistributedEntity
    {
        public ImmutableHashSet<T> Adds { get; protected set; }

        public ImmutableHashSet<T> Removes { get; protected set; }

        protected P_SetBase()
        {
            Adds = ImmutableHashSet<T>.Empty;
            Removes = ImmutableHashSet<T>.Empty;
        }

        protected P_SetBase(ImmutableHashSet<T> adds, ImmutableHashSet<T> removes)
        {
            Adds = adds;
            Removes = removes;
        }

        public bool Lookup(T value)
        {
            if (Removes.Any(r => Equals(r, value)))
            {
                return false;
            }

            return Adds.Any(r => Equals(r, value));
        }
    }
}