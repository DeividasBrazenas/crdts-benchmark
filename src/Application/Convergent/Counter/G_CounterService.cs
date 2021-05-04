using System;
using System.Collections.Generic;
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

                var counter = new G_Counter(existingElements.ToImmutableHashSet());

                counter = counter.Add(value, id);

                _repository.PersistValues(counter.Elements);
            }
        }

        public void Merge(IEnumerable<CounterElement> elements)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetValues();

                var counter = new G_Counter(existingElements.ToImmutableHashSet());

                var mergedCounter = counter.Merge(elements.ToImmutableHashSet());

                _repository.PersistValues(mergedCounter.Elements);
            }
        }

        public int Sum()
        {
            var existingElements = _repository.GetValues();

            var counter = new G_Counter(existingElements.ToImmutableHashSet());

            return counter.Sum();
        }

        public IEnumerable<CounterElement> State => _repository.GetValues();
    }
}