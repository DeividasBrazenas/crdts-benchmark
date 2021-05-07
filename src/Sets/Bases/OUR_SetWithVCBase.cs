using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Bases
{
    public abstract class OUR_SetWithVCBase<T> where T : DistributedEntity
    {
        public ImmutableHashSet<OUR_SetWithVCElement<T>> Adds { get; protected set; }

        public ImmutableHashSet<OUR_SetWithVCElement<T>> Removes { get; protected set; }

        protected OUR_SetWithVCBase()
        {
            Adds = ImmutableHashSet<OUR_SetWithVCElement<T>>.Empty;
            Removes = ImmutableHashSet<OUR_SetWithVCElement<T>>.Empty;
        }

        protected OUR_SetWithVCBase(ImmutableHashSet<OUR_SetWithVCElement<T>> adds, ImmutableHashSet<OUR_SetWithVCElement<T>> removes)
        {
            Adds = adds;
            Removes = removes;
        }

        public ImmutableHashSet<OUR_SetWithVCElement<T>> Elements =>
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