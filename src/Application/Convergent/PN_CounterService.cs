using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Counters.Convergent;
using CRDT.Counters.Entities;

namespace CRDT.Application.Convergent
{
    public class PN_CounterService
    {
        private readonly IPN_CounterRepository _repository;

        public PN_CounterService(IPN_CounterRepository repository)
        {
            _repository = repository;
        }

        public void Merge(IEnumerable<CounterElement> additions, IEnumerable<CounterElement> subtractions)
        {
            var existingAdditions = _repository.GetAdditions();
            var existingSubtractions = _repository.GetSubtractions();

            var counter = new PN_Counter(existingAdditions.ToImmutableHashSet(), existingSubtractions.ToImmutableHashSet());

            var mergedCounter = counter.Merge(additions.ToImmutableHashSet(), subtractions.ToImmutableHashSet());

            _repository.PersistAdditions(mergedCounter.Additions);
            _repository.PersistSubtractions(mergedCounter.Subtractions);
        }

        public int Sum()
        {
            var existingAdditions = _repository.GetAdditions();
            var existingSubtractions = _repository.GetSubtractions();

            var counter = new PN_Counter(existingAdditions.ToImmutableHashSet(), existingSubtractions.ToImmutableHashSet());

            return counter.Sum;
        }
    }
}