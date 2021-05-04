using System;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Counters.Commutative.PositiveNegative;

namespace CRDT.Application.Commutative.Counter
{
    public class PN_CounterService
    {
        private readonly IPN_CounterRepository _repository;
        private readonly object _lockObject = new();

        public PN_CounterService(IPN_CounterRepository repository)
        {
            _repository = repository;
        }

        public void Add(int value, Guid nodeId)
        {
            lock (_lockObject)
            {
                var existingAdditions = _repository.GetAdditions();
                var existingSubtractions = _repository.GetSubtractions();

                var counter = new PN_Counter(existingAdditions.ToImmutableHashSet(), existingSubtractions.ToImmutableHashSet());

                var mergedCounter = counter.Add(value, nodeId);

                _repository.PersistAdditions(mergedCounter.Additions);
            }
        }

        public void Subtract(int value, Guid nodeId)
        {
            lock (_lockObject)
            {
                var existingAdditions = _repository.GetAdditions();
                var existingSubtractions = _repository.GetSubtractions();

                var counter = new PN_Counter(existingAdditions.ToImmutableHashSet(), existingSubtractions.ToImmutableHashSet());

                var mergedCounter = counter.Subtract(value, nodeId);

                _repository.PersistSubtractions(mergedCounter.Subtractions);
            }
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