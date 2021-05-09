using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Convergent.TwoPhase;
using CRDT.Sets.Entities;

namespace CRDT.Application.Convergent.Set
{
    public class P_OptimizedSetService<T> where T : DistributedEntity
    {
        private readonly IP_OptimizedSetRepository<T> _repository;
        private readonly object _lockObject = new();

        public P_OptimizedSetService(IP_OptimizedSetRepository<T> repository)
        {
            _repository = repository;
        }

        public void LocalAdd(T value)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new P_OptimizedSet<T>(existingElements);

                set = set.Add(value);

                _repository.PersistElements(set.Elements);
            }
        }

        public void LocalRemove(T value)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new P_OptimizedSet<T>(existingElements);

                set = set.Remove(value);

                _repository.PersistElements(set.Elements);
            }
        }

        public void Merge(ImmutableHashSet<P_OptimizedSetElement<T>> elements)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new P_OptimizedSet<T>(existingElements);

                set = set.Merge(elements);

                _repository.PersistElements(set.Elements);
            }
        }

        public bool Lookup(T value)
        {
            var existingElements = _repository.GetElements();

            var set = new P_OptimizedSet<T>(existingElements);

            var lookup = set.Lookup(value);

            return lookup;
        }

        public ImmutableHashSet<P_OptimizedSetElement<T>> State => _repository.GetElements();
    }
}