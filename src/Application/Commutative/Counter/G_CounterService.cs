using System;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Counters.Commutative.GrowOnly;

namespace CRDT.Application.Commutative.Counter
{
    public class G_CounterService
    {
        public readonly IG_CounterRepository _repository;
        private readonly object _lockObject = new();

        public G_CounterService(IG_CounterRepository repository)
        {
            _repository = repository;
        }

        public void Add(int value, Guid nodeId)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetValues();

                var counter = new G_Counter(existingElements.ToImmutableHashSet());

                var mergedCounter = counter.Add(value, nodeId);

                _repository.PersistValues(mergedCounter.Elements);
            }
        }

        public int Sum()
        {
            var existingElements = _repository.GetValues();

            var counter = new G_Counter(existingElements.ToImmutableHashSet());

            return counter.Sum();
        }
    }
}