using System.Collections.Generic;
using CRDT.Application.Interfaces;
using CRDT.Counters.Entities;

namespace CRDT.Application.UnitTests.Repositories
{
    public class G_CounterRepository : IG_CounterRepository
    {
        public IEnumerable<CounterElement> Elements { get; private set; }

        public G_CounterRepository()
        {
            Elements = new List<CounterElement>();
        }

        public IEnumerable<CounterElement> GetValues()
        {
            return Elements;
        }

        public void PersistValues(IEnumerable<CounterElement> values)
        {
            Elements = values;
        }
    }
}