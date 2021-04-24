using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Commutative;

namespace CRDT.Application.Commutative
{
    public class G_SetService<T> where T : DistributedEntity
    {
        private readonly INewRepository<T> _repository;

        public G_SetService(INewRepository<T> repository)
        {
            _repository = repository;
        }

        public void Merge(T value)
        {
            var existingEntities = _repository.GetValues();

            var set = new G_Set<T>(existingEntities.ToImmutableHashSet());

            set = set.Merge(value);

            _repository.AddValues(set.Values);
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