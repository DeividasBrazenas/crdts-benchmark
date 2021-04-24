using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Core.Cluster;
using CRDT.Sets.Convergent;
using CRDT.Sets.Entities;

namespace CRDT.Application.Convergent
{
    public class OR_SetService<T> where T : DistributedEntity
    {
        private readonly IOR_SetRepository<T> _repository;

        public OR_SetService(IOR_SetRepository<T> repository)
        {
            _repository = repository;
        }

        public void Add(T value, Node node)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OR_Set<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            var element = new OR_SetElement<T>(value, node.Id);

            set = set.Add(element);

            _repository.PersistAdds(set.Adds);
        }

        public void Remove(T value, Node node)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OR_Set<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            var element = new OR_SetElement<T>(value, node.Id);

            set = set.Remove(element);

            _repository.PersistRemoves(set.Removes);
        }

        public void Merge(IEnumerable<OR_SetElement<T>> adds, IEnumerable<OR_SetElement<T>> removes)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OR_Set<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            set = set.Merge(adds.ToImmutableHashSet(), removes.ToImmutableHashSet());

            _repository.PersistAdds(set.Adds);
            _repository.PersistRemoves(set.Removes);
        }

        public bool Lookup(T value)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OR_Set<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            var lookup = set.Lookup(value);

            return lookup;
        }
    }
}