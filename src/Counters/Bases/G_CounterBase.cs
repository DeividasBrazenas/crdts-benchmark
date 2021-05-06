using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Counters.Entities;

namespace CRDT.Counters.Bases
{
    public abstract class G_CounterBase : ValueObject
    {
        public ImmutableHashSet<CounterElement> Elements;

        protected G_CounterBase(ImmutableHashSet<CounterElement> elements)
        {
            Elements = elements;
        }

        public int Sum() => Elements.Sum(e => e.Value);
    }
}