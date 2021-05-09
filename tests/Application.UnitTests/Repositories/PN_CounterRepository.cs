using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Counters.Entities;

namespace CRDT.Application.UnitTests.Repositories
{
    public class PN_CounterRepository : IPN_CounterRepository
    {
        public ImmutableHashSet<CounterElement> Additions;

        public ImmutableHashSet<CounterElement> Subtractions;

        public PN_CounterRepository()
        {
            Additions = ImmutableHashSet<CounterElement>.Empty;
            Subtractions = ImmutableHashSet<CounterElement>.Empty;
        }

        public ImmutableHashSet<CounterElement> GetAdditions()
        {
            return Additions;
        }

        public ImmutableHashSet<CounterElement> GetSubtractions()
        {
            return Subtractions;
        }

        public void PersistAdditions(ImmutableHashSet<CounterElement> additions)
        {
            Additions = additions;
        }

        public void PersistSubtractions(ImmutableHashSet<CounterElement> subtractions)
        {
            Subtractions = subtractions;
        }
    }
}