using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Convergent.GrowOnly;

namespace CRDT.Application.Convergent.Set
{
    public class G_SetService<T> where T : DistributedEntity
    {
        private readonly IG_SetRepository<T> _repository;
        private readonly object _lockObject = new();

        public G_SetService(IG_SetRepository<T> repository)
        {
            _repository = repository;
        }

        public void LocalAdd(T value)
        {
            lock (_lockObject)
            {
                var existingEntities = _repository.GetValues();

                var set = new G_Set<T>(existingEntities);

                set = set.Add(value);

                _repository.PersistValues(set.Values);
            }
        }

        public void Merge(ImmutableHashSet<T> values)
        {
            lock (_lockObject)
            {
                var existingEntities = _repository.GetValues();

                var set = new G_Set<T>(existingEntities);

                set = set.Merge(values);

                _repository.PersistValues(set.Values);
            }
        }

        public bool Lookup(T value)
        {
            var existingEntities = _repository.GetValues();

            var set = new G_Set<T>(existingEntities);

            var lookup = set.Lookup(value);

            return lookup;
        }

        public ImmutableHashSet<T> State => _repository.GetValues();
    }
}