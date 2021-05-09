using System.Collections.Immutable;
using CRDT.Counters.Entities;

namespace CRDT.Application.Interfaces
{
    public interface IPN_CounterRepository
    {
        ImmutableHashSet<CounterElement> GetAdditions();

        ImmutableHashSet<CounterElement> GetSubtractions();

        void PersistAdditions(ImmutableHashSet<CounterElement> additions);

        void PersistSubtractions(ImmutableHashSet<CounterElement> subtractions);
    }
}