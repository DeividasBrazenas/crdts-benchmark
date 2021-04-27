using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Counters.Convergent;
using CRDT.Counters.Entities;

namespace CRDT.Application.Convergent
{
    public class G_CounterService
    {
        private readonly IG_CounterRepository _repository;

        public G_CounterService(IG_CounterRepository repository)
        {
            _repository = repository;
        }

        public void Merge(IEnumerable<CounterElement> elements)
        {
            var existingElements = _repository.GetValues();

            var counter = new G_Counter(existingElements.ToImmutableHashSet());

            var mergedCounter = counter.Merge(elements.ToImmutableHashSet());

            _repository.PersistValues(mergedCounter.Elements);
        }

        public int Sum()
        {
            var existingElements = _repository.GetValues();

            var counter = new G_Counter(existingElements.ToImmutableHashSet());

            return counter.Sum;
        }
    }
}