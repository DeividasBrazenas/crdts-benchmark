using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Commutative.TwoPhase;

namespace CRDT.Application.Commutative.Set
{
    public class U_SetService<T> where T : DistributedEntity
    {
        private readonly IU_SetRepository<T> _repository;

        public U_SetService(IU_SetRepository<T> repository)
        {
            _repository = repository;
        }

        public void Add(T value)
        {
            var existingElements = _repository.GetElements();

            var set = new U_Set<T>(existingElements.ToImmutableHashSet());

            set = set.Add(value);

            _repository.PersistElements(set.Elements);
        }

        public void Remove(T value)
        {
            var existingElements = _repository.GetElements();

            var set = new U_Set<T>(existingElements.ToImmutableHashSet());

            set = set.Remove(value);

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