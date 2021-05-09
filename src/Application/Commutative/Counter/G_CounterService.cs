using System;
using CRDT.Application.Interfaces;
using CRDT.Counters.Commutative.GrowOnly;

namespace CRDT.Application.Commutative.Counter
{
    public class G_CounterService
    {
        private readonly IG_CounterRepository _repository;
        private readonly object _lockObject = new();

        public G_CounterService(IG_CounterRepository repository)
        {
            _repository = repository;
        }

        public void LocalAdd(int value, Guid nodeId)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetValues();

                var counter = new G_Counter(existingElements);

                var mergedCounter = counter.Add(value, nodeId);

                _repository.PersistValues(mergedCounter.Elements);
            }
        }

        public void DownstreamAdd(int value, Guid nodeId)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetValues();

                var counter = new G_Counter(existingElements);

                var mergedCounter = counter.Add(value, nodeId);

                _repository.PersistValues(mergedCounter.Elements);
            }
        }

        public int Sum()
        {
            var existingElements = _repository.GetValues();

            var counter = new G_Counter(existingElements);

            return counter.Sum();
        }
    }
}