using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Convergent.LastWriterWins;
using CRDT.Sets.Entities;

namespace CRDT.Application.Convergent.Set
{
    public class LWW_OptimizedSetWithVCService<T> where T : DistributedEntity
    {
        private readonly ILWW_OptimizedSetWithVCRepository<T> _repository;
        private readonly object _lockObject = new();

        public LWW_OptimizedSetWithVCService(ILWW_OptimizedSetWithVCRepository<T> repository)
        {
            _repository = repository;
        }

        public void LocalAssign(T value, VectorClock vectorClock)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new LWW_OptimizedSetWithVC<T>(existingElements);

                set = set.Assign(value, vectorClock);

                _repository.PersistElements(set.Elements);
            }
        }

        public void LocalRemove(T value, VectorClock vectorClock)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new LWW_OptimizedSetWithVC<T>(existingElements);

                set = set.Remove(value, vectorClock);

                _repository.PersistElements(set.Elements);
            }
        }

        public void Merge(ImmutableHashSet<LWW_OptimizedSetWithVCElement<T>> elements)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new LWW_OptimizedSetWithVC<T>(existingElements);

                set = set.Merge(elements);

                _repository.PersistElements(set.Elements);
            }
        }

        public bool Lookup(T value)
        {
            var existingElements = _repository.GetElements();

            var set = new LWW_OptimizedSetWithVC<T>(existingElements);

            var lookup = set.Lookup(value);

            return lookup;
        }

        public ImmutableHashSet<LWW_OptimizedSetWithVCElement<T>> State => _repository.GetElements();
    }
}