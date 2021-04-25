using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Commutative;
using CRDT.Sets.Entities;

namespace CRDT.Application.Commutative
{
    public class LWW_SetService<T> where T : DistributedEntity
    {
        private readonly ILWW_SetRepository<T> _repository;

        public LWW_SetService(ILWW_SetRepository<T> repository)
        {
            _repository = repository;
        }

        public void Add(T value, long timestamp)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new LWW_Set<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            var element = new LWW_SetElement<T>(value, timestamp);
            set = set.Add(element);

            _repository.PersistAdds(set.Adds);
        }

        public void Update(T value, long timestamp)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new LWW_Set<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            var element = new LWW_SetElement<T>(value, timestamp);

            set = set.Update(element);

            _repository.PersistAdds(set.Adds);
        }

        public void Remove(T value, long timestamp)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new LWW_Set<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            var element = new LWW_SetElement<T>(value, timestamp);
            set = set.Remove(element);

            _repository.PersistRemoves(set.Removes);
        }

        public bool Lookup(T value)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new LWW_Set<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            var lookup = set.Lookup(value);

            return lookup;
        }
    }
}