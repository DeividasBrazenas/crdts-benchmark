using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Counters.Entities;

namespace CRDT.Application.UnitTests.Repositories
{
    public class G_CounterRepository : IG_CounterRepository
    {
        public ImmutableHashSet<CounterElement> Elements { get; private set; }

        public G_CounterRepository()
        {
            Elements = ImmutableHashSet<CounterElement>.Empty;
        }

        public ImmutableHashSet<CounterElement> GetValues()
        {
            return Elements;
        }

        public void PersistValues(ImmutableHashSet<CounterElement> values)
        {
            Elements = values;
        }
    }
}