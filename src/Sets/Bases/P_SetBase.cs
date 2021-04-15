using System.Collections.Immutable;
using CRDT.Core.Abstractions;

namespace CRDT.Sets.Bases
{
    public abstract class P_SetBase<T> where T : DistributedEntity
    {
        public IImmutableSet<T> Adds { get; protected set; }

        public IImmutableSet<T> Removes { get; protected set; }

        protected P_SetBase(IImmutableSet<T> adds, IImmutableSet<T> removes)
        {
            Adds = adds;
            Removes = removes;
        }
    }
}