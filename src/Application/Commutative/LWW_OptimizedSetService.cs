using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Commutative;

namespace CRDT.Application.Commutative
{
    public class LWW_OptimizedSetService<T> where T : DistributedEntity
    {
        private readonly ILWW_OptimizedSetRepository<T> _repository;

        public LWW_OptimizedSetService(ILWW_OptimizedSetRepository<T> repository)
        {
            _repository = repository;
        }

        public void Add(T value, long timestamp)
        {
            var existingElements = _repository.GetElements();

            var set = new LWW_OptimizedSet<T>(existingElements.ToImmutableHashSet());

            set = set.Add(value, timestamp);

            _repository.PersistElements(set.Elements);
        }

        public void Remove(T value, long timestamp)
        {
            var existingElements = _repository.GetElements();

            var set = new LWW_OptimizedSet<T>(existingElements.ToImmutableHashSet());

            set = set.Remove(value, timestamp);

            _repository.PersistElements(set.Elements);
        }

        public bool Lookup(T value)
        {
            var existingElements = _repository.GetElements();

            var set = new LWW_OptimizedSet<T>(existingElements.ToImmutableHashSet());

            var lookup = set.Lookup(value);

            return lookup;
        }
    }
}