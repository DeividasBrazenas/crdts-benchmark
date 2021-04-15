using System.Collections.Immutable;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Bases
{
    public abstract class OUR_SetBase<T> where T : DistributedEntity
    {
        public IImmutableSet<OUR_SetElement<T>> Adds { get; protected set; }

        public IImmutableSet<OUR_SetElement<T>> Removes { get; protected set; }

        protected OUR_SetBase()
        {
            Adds = ImmutableHashSet<OUR_SetElement<T>>.Empty;
            Removes = ImmutableHashSet<OUR_SetElement<T>>.Empty;
        }

        protected OUR_SetBase(IImmutableSet<OUR_SetElement<T>> adds, IImmutableSet<OUR_SetElement<T>> removes)
        {
            Adds = adds;
            Removes = removes;
        }
    }
}