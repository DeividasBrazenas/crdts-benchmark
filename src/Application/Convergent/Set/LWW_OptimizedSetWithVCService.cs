using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Convergent.LastWriterWins;
using CRDT.Sets.Entities;

namespace CRDT.Application.Convergent.Set
{
    public class LWW_OptimizedSetWithVCService<T> where T : DistributedEntity
    {
        private readonly ILWW_OptimizedSetWithVCRepository<T> _repository;

        public LWW_OptimizedSetWithVCService(ILWW_OptimizedSetWithVCRepository<T> repository)
        {
            _repository = repository;
        }

        public void Merge(IEnumerable<LWW_OptimizedSetWithVCElement<T>> elements)
        {
            var existingElements = _repository.GetElements();

            var set = new LWW_OptimizedSetWithVC<T>(existingElements.ToImmutableHashSet());

            set = set.Merge(elements.ToImmutableHashSet());

            _repository.PersistElements(set.Elements);
        }

        public bool Lookup(T value)
        {
            var existingElements = _repository.GetElements();

            var set = new LWW_OptimizedSetWithVC<T>(existingElements.ToImmutableHashSet());

            var lookup = set.Lookup(value);

            return lookup;
        }
    }
}