using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Commutative.LastWriterWins;

namespace CRDT.Application.Commutative.Set
{
    public class LWW_OptimizedSetWithVCService<T> where T : DistributedEntity
    {
        private readonly ILWW_OptimizedSetWithVCRepository<T> _repository;

        public LWW_OptimizedSetWithVCService(ILWW_OptimizedSetWithVCRepository<T> repository)
        {
            _repository = repository;
        }

        public void Add(T value, VectorClock vectorClock)
        {
            var existingElements = _repository.GetElements();

            var set = new LWW_OptimizedSetWithVC<T>(existingElements.ToImmutableHashSet());

            set = set.Add(value, vectorClock);

            _repository.PersistElements(set.Elements);
        }

        public void Remove(T value, VectorClock vectorClock)
        {
            var existingElements = _repository.GetElements();

            var set = new LWW_OptimizedSetWithVC<T>(existingElements.ToImmutableHashSet());

            set = set.Remove(value, vectorClock);

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