using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;

namespace CRDT.Sets.Bases
{
    public abstract class G_SetBase<T> where T : DistributedEntity
    {
        public ImmutableHashSet<T> Values { get; }

        protected G_SetBase()
        {
            Values = ImmutableHashSet<T>.Empty;
        }

        protected G_SetBase(ImmutableHashSet<T> values)
        {
            Values = values;
        }

        public bool Lookup(T value) => Values.Any(v => Equals(v, value));
    }
}