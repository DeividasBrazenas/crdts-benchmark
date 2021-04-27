using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Counters.Entities;

namespace CRDT.Counters.Bases
{
    public abstract class PN_CounterBase : ValueObject
    {
        public IImmutableSet<CounterElement> Additions;

        public IImmutableSet<CounterElement> Subtractions;

        protected PN_CounterBase(IImmutableSet<CounterElement> additions, IImmutableSet<CounterElement> subtractions)
        {
            Additions = additions;
            Subtractions = subtractions;
        }

        public int Sum => Additions.Sum(e => e.Value) - Subtractions.Sum(e => e.Value);
    }
}