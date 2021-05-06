using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Commutative.GrowOnly;

namespace CRDT.Application.Commutative.Set
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

        public void DownstreamAdd(T value)
        {
            lock (_lockObject)
            {
                var existingEntities = _repository.GetValues();

                var set = new G_Set<T>(existingEntities);

                set = set.Add(value);

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

        public IEnumerable<T> State => _repository.GetValues();
    }
}