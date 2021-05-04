using System.Collections.Generic;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Counters.Entities;

namespace CRDT.Benchmarks.Repositories
{
    public class PN_CounterRepository : IPN_CounterRepository
    {
        public IEnumerable<CounterElement> Additions;

        public IEnumerable<CounterElement> Subtractions;

        public PN_CounterRepository()
        {
            Additions = new List<CounterElement>();
            Subtractions = new List<CounterElement>();
        }

        public IEnumerable<CounterElement> GetAdditions()
        {
            return Additions;
        }

        public IEnumerable<CounterElement> GetSubtractions()
        {
            return Subtractions;
        }

        public void PersistAdditions(IEnumerable<CounterElement> additions)
        {
            Additions = additions;
        }

        public void PersistSubtractions(IEnumerable<CounterElement> subtractions)
        {
            Subtractions = subtractions;
        }
    }
}