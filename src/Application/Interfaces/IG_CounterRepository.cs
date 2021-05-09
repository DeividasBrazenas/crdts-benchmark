using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Counters.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IG_CounterRepository
    {
        ImmutableHashSet<CounterElement> GetValues();

        void PersistValues(ImmutableHashSet<CounterElement> values);
    }
}