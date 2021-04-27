using System;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Counters.Commutative;

namespace CRDT.Application.Commutative
{
    public class PN_CounterService
    {
        private readonly IPN_CounterRepository _repository;

        public PN_CounterService(IPN_CounterRepository repository)
        {
            _repository = repository;
        }

        public void Add(int value, Guid nodeId)
        {
            var existingAdditions = _repository.GetAdditions();
            var existingSubtractions = _repository.GetSubtractions();

            var counter = new PN_Counter(existingAdditions.ToImmutableHashSet(), existingSubtractions.ToImmutableHashSet());

            var mergedCounter = counter.Add(value, nodeId);

            _repository.PersistAdditions(mergedCounter.Additions);
        }

        public void Subtract(int value, Guid nodeId)
        {
            var existingAdditions = _repository.GetAdditions();
            var existingSubtractions = _repository.GetSubtractions();

            var counter = new PN_Counter(existingAdditions.ToImmutableHashSet(), existingSubtractions.ToImmutableHashSet());

            var mergedCounter = counter.Subtract(value, nodeId);

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