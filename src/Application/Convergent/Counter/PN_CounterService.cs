using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Counters.Convergent.PositiveNegative;
using CRDT.Counters.Entities;

namespace CRDT.Application.Convergent.Counter
{
    public class PN_CounterService
    {
        private readonly IPN_CounterRepository _repository;
        private readonly object _lockObject = new();

        public PN_CounterService(IPN_CounterRepository repository)
        {
            _repository = repository;
        }

        public void LocalAdd(int value, Guid nodeId)
        {
            lock (_lockObject)
            {
                var existingAdditions = _repository.GetAdditions();
                var existingSubtractions = _repository.GetSubtractions();

                var counter = new PN_Counter(existingAdditions, existingSubtractions);

                var mergedCounter = counter.Add(value, nodeId);

                _repository.PersistAdditions(mergedCounter.Additions);
            }
        }

        public void LocalSubtract(int value, Guid nodeId)
        {
            lock (_lockObject)
            {
                var existingAdditions = _repository.GetAdditions();
                var existingSubtractions = _repository.GetSubtractions();

                var counter = new PN_Counter(existingAdditions, existingSubtractions);

                var mergedCounter = counter.Subtract(value, nodeId);

                _repository.PersistSubtractions(mergedCounter.Subtractions);
            }
        }

        public void Merge(ImmutableHashSet<CounterElement> additions, ImmutableHashSet<CounterElement> subtractions)
        {
            lock (_lockObject)
            {
                var existingAdditions = _repository.GetAdditions();
                var existingSubtractions = _repository.GetSubtractions();

                var counter = new PN_Counter(existingAdditions, existingSubtractions);

                var mergedCounter = counter.Merge(additions, subtractions);

                _repository.PersistAdditions(mergedCounter.Additions);
                _repository.PersistSubtractions(mergedCounter.Subtractions);
            }
        }

        public int Sum()
        {
            var existingAdditions = _repository.GetAdditions();
            var existingSubtractions = _repository.GetSubtractions();

            var counter = new PN_Counter(existingAdditions, existingSubtractions);

            return counter.Sum;
        }

        public (ImmutableHashSet<CounterElement>, ImmutableHashSet<CounterElement>) State => (_repository.GetAdditions(), _repository.GetSubtractions());
    }
}