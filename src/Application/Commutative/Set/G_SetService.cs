using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Commutative.GrowOnly;

namespace CRDT.Application.Commutative.Set
{
    public class G_SetService<T> where T : DistributedEntity
    {
        private readonly IG_SetRepository<T> _repository;

        public G_SetService(IG_SetRepository<T> repository)
        {
            _repository = repository;
        }

        public void Add(T value)
        {
            var existingEntities = _repository.GetValues();

            var set = new G_Set<T>(existingEntities.ToImmutableHashSet());

            set = set.Add(value);

            _repository.PersistValues(set.Values);
        }

        public bool Lookup(T value)
        {
            var existingEntities = _repository.GetValues();

            var set = new G_Set<T>(existingEntities.ToImmutableHashSet());

            var lookup = set.Lookup(value);

            return lookup;
        }
    }
}