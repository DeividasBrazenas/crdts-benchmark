using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Bases
{
    public abstract class OR_SetBase<T> where T : DistributedEntity
    {
        public ImmutableHashSet<OR_SetElement<T>> Adds { get; protected set; }

        public ImmutableHashSet<OR_SetElement<T>> Removes { get; protected set; }

        protected OR_SetBase()
        {
            Adds = ImmutableHashSet<OR_SetElement<T>>.Empty;
            Removes = ImmutableHashSet<OR_SetElement<T>>.Empty;
        }

        protected OR_SetBase(ImmutableHashSet<OR_SetElement<T>> adds, ImmutableHashSet<OR_SetElement<T>> removes)
        {
            Adds = adds;
            Removes = removes;
        }

        public ImmutableHashSet<T> Values =>
            Adds
                .Where(a => !Removes.Any(r => Equals(r, a) && a.Tag == r.Tag))
                .Select(e => e.Value)
                .Distinct()
                .ToImmutableHashSet();

        public bool Lookup(T value) => Values.Any(v => Equals(v, value));
    }
}