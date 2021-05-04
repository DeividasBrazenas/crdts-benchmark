using System.Collections.Generic;
using CRDT.Counters.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IPN_CounterRepository
    {
        IEnumerable<CounterElement> GetAdditions();

        IEnumerable<CounterElement> GetSubtractions();

        void PersistAdditions(IEnumerable<CounterElement> additions);

        void PersistSubtractions(IEnumerable<CounterElement> subtractions);
    }
}