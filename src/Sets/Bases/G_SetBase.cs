using System.Collections.Immutable;
using CRDT.Core.Abstractions;

namespace CRDT.Sets.Bases
{
    public abstract class G_SetBase<T> where T : DistributedEntity
    {
        public IImmutableSet<T> Values { get; protected set; }

        protected G_SetBase(IImmutableSet<T> values)
        {
            Values = values;
        }
    }
}