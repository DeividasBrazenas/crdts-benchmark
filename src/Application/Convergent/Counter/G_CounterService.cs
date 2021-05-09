using System;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Counters.Convergent.GrowOnly;
using CRDT.Counters.Entities;

namespace CRDT.Application.Convergent.Counter
{
    public class G_CounterService
    {
        private readonly IG_CounterRepository _repository;
        private readonly object _lockObject = new();

        public G_CounterService(IG_CounterRepository repository)
        {
            _repository = repository;
        }

        public void LocalAdd(int value, Guid id)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetValues();

                var counter = new G_Counter(existingElements);

                counter = counter.Add(value, id);

                _repository.PersistValues(counter.Elements);
            }
        }

        public void Merge(ImmutableHashSet<CounterElement> elements)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetValues();

                var counter = new G_Counter(existingElements);

                var mergedCounter = counter.Merge(elements);

                _repository.PersistValues(mergedCounter.Elements);
            }
        }

        public int Sum()
        {
            var existingElements = _repository.GetValues();

            var counter = new G_Counter(existingElements);

            return counter.Sum();
        }

        public ImmutableHashSet<CounterElement> State => _repository.GetValues();
    }
}