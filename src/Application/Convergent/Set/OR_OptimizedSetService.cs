using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Convergent.ObservedRemoved;
using CRDT.Sets.Entities;

namespace CRDT.Application.Convergent.Set
{
    public class OR_OptimizedSetService<T> where T : DistributedEntity
    {
        private readonly IOR_OptimizedSetRepository<T> _repository;

        public OR_OptimizedSetService(IOR_OptimizedSetRepository<T> repository)
        {
            _repository = repository;
        }

        public void Merge(IEnumerable<OR_OptimizedSetElement<T>> elements)
        {
            var existingElements = _repository.GetElements();

            var set = new OR_OptimizedSet<T>(existingElements.ToImmutableHashSet());

            set = set.Merge(elements.ToImmutableHashSet());

            _repository.PersistElements(set.Elements);
        }

        public bool Lookup(T value)
        {
            var existingElements = _repository.GetElements();

            var set = new OR_OptimizedSet<T>(existingElements.ToImmutableHashSet());

            var lookup = set.Lookup(value);

            return lookup;
        }
    }
}