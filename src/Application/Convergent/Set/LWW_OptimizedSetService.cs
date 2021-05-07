using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Convergent.LastWriterWins;
using CRDT.Sets.Entities;

namespace CRDT.Application.Convergent.Set
{
    public class LWW_OptimizedSetService<T> where T : DistributedEntity
    {
        private readonly ILWW_OptimizedSetRepository<T> _repository;
        private readonly object _lockObject = new();

        public LWW_OptimizedSetService(ILWW_OptimizedSetRepository<T> repository)
        {
            _repository = repository;
        }

        public void LocalAdd(T value, long timestamp)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new LWW_OptimizedSet<T>(existingElements);

                set = set.Add(value, timestamp);

                _repository.PersistElements(set.Elements);
            }
        }

        public void LocalRemove(T value, long timestamp)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new LWW_OptimizedSet<T>(existingElements);

                set = set.Remove(value, timestamp);

                _repository.PersistElements(set.Elements);
            }
        }

        public void Merge(ImmutableHashSet<LWW_OptimizedSetElement<T>> elements)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new LWW_OptimizedSet<T>(existingElements);

                set = set.Merge(elements);

                _repository.PersistElements(set.Elements);
            }
        }

        public bool Lookup(T value)
        {
            var existingElements = _repository.GetElements();

            var set = new LWW_OptimizedSet<T>(existingElements);

            var lookup = set.Lookup(value);

            return lookup;
        }

        public ImmutableHashSet<LWW_OptimizedSetElement<T>> State => _repository.GetElements();
    }
}