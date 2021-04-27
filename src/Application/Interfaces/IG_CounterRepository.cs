using System.Collections.Generic;
using CRDT.Counters.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IG_CounterRepository
    {
        IEnumerable<CounterElement> GetValues();

        void PersistValues(IEnumerable<CounterElement> values);
    }
}