using System.Collections.Immutable;
using System.Linq;
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

        public IImmutableSet<T> Values =>
            Adds
                .Where(a => !Removes.Any(r => Equals(r, a) && a.Tag == r.Tag))
                .Select(e => e.Value)
                .ToImmutableHashSet();

        public bool Lookup(T value) => Values.Any(v => Equals(v, value));
    }
}