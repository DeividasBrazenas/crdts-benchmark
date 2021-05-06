using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Commutative.TwoPhase;
using CRDT.Sets.Entities;

namespace CRDT.Application.Commutative.Set
{
    public class U_SetService<T> where T : DistributedEntity
    {
        private readonly IU_SetRepository<T> _repository;
        private readonly object _lockObject = new();

        public U_SetService(IU_SetRepository<T> repository)
        {
            _repository = repository;
        }

        public void LocalAdd(T value)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new U_Set<T>(existingElements);

                set = set.Add(value);

                _repository.PersistElements(set.Elements);
            }
        }

        public void LocalRemove(T value)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new U_Set<T>(existingElements);

                set = set.Remove(value);

                _repository.PersistElements(set.Elements);
            }
        }

        public void DownstreamAdd(T value)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new U_Set<T>(existingElements);

                set = set.Add(value);

                _repository.PersistElements(set.Elements);
            }
        }

        public void DownstreamRemove(T value)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new U_Set<T>(existingElements);

                set = set.Remove(value);

                _repository.PersistElements(set.Elements);
            }
        }

        public bool Lookup(T value)
        {
            var existingElements = _repository.GetElements();

            var set = new U_Set<T>(existingElements);

            var lookup = set.Lookup(value);

            return lookup;
        }

        public ImmutableHashSet<U_SetElement<T>> State => _repository.GetElements();
    }
}