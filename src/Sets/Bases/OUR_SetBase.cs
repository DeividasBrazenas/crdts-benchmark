using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Bases
{
    public abstract class OUR_SetBase<T> where T : DistributedEntity
    {
        public ImmutableHashSet<OUR_SetElement<T>> Adds { get; protected set; }

        public ImmutableHashSet<OUR_SetElement<T>> Removes { get; protected set; }

        protected OUR_SetBase()
        {
            Adds = ImmutableHashSet<OUR_SetElement<T>>.Empty;
            Removes = ImmutableHashSet<OUR_SetElement<T>>.Empty;
        }

        protected OUR_SetBase(ImmutableHashSet<OUR_SetElement<T>> adds, ImmutableHashSet<OUR_SetElement<T>> removes)
        {
            Adds = adds;
            Removes = removes;
        }

        public ImmutableHashSet<OUR_SetElement<T>> Elements =>
            Adds
                .Where(a => !Removes.Any(r => Equals(r, a) && a.Tag == r.Tag))
                .Distinct()
                .ToImmutableHashSet();

        public ImmutableHashSet<T> Values =>
            Elements
                .Select(e => e.Value)
                .ToImmutableHashSet();

        public bool Lookup(T value) => Values.Any(v => Equals(v, value));
    }
}