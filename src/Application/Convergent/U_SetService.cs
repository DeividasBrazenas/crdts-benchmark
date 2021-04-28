using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Convergent;
using CRDT.Sets.Entities;

namespace CRDT.Application.Convergent
{
    public class U_SetService<T> where T : DistributedEntity
    {
        private readonly IU_SetRepository<T> _repository;

        public U_SetService(IU_SetRepository<T> repository)
        {
            _repository = repository;
        }

        public void Merge(IEnumerable<U_SetElement<T>> elements)
        {
            var existingElements = _repository.GetElements();

            var set = new U_Set<T>(existingElements.ToImmutableHashSet());

            set = set.Merge(elements.ToImmutableHashSet());

            _repository.PersistElements(set.Elements);
        }

        public bool Lookup(T value)
        {
            var existingElements = _repository.GetElements();

            var set = new U_Set<T>(existingElements.ToImmutableHashSet());

            var lookup = set.Lookup(value);

            return lookup;
        }
    }
}