using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Bases
{
    public abstract class OR_SetBase<T> where T : DistributedEntity
    {
        public IImmutableSet<OR_SetElement<T>> Adds { get; protected set; }

        public IImmutableSet<OR_SetElement<T>> Removes { get; protected set; }

        protected OR_SetBase()
        {
            Adds = ImmutableHashSet<OR_SetElement<T>>.Empty;
            Removes = ImmutableHashSet<OR_SetElement<T>>.Empty;
        }

        protected OR_SetBase(IImmutableSet<OR_SetElement<T>> adds, IImmutableSet<OR_SetElement<T>> removes)
        {
            Adds = adds;
            Removes = removes;
        }

        public IImmutableSet<T> Values =>
            Adds
                .Where(a => !Removes.Any(r => Equals(r, a) && a.Tag == r.Tag))
                .Select(e => e.Value)
                .Distinct()
                .ToImmutableHashSet();

        public bool Lookup(T value) => Values.Any(v => Equals(v, value));
    }
}