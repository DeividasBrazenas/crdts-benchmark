using System.Collections.Generic;
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

        public LWW_OptimizedSetService(ILWW_OptimizedSetRepository<T> repository)
        {
            _repository = repository;
        }

        public void Merge(IEnumerable<LWW_OptimizedSetElement<T>> elements)
        {
            var existingElements = _repository.GetElements();

            var set = new LWW_OptimizedSet<T>(existingElements.ToImmutableHashSet());

            set = set.Merge(elements.ToImmutableHashSet());

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