using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Core.Cluster;
using CRDT.Sets.Commutative;

namespace CRDT.Application.Commutative
{
    public class OUR_SetService<T> where T : DistributedEntity
    {
        private readonly IOUR_SetRepository<T> _repository;

        public OUR_SetService(IOUR_SetRepository<T> repository)
        {
            _repository = repository;
        }

        public void Add(T value, Guid tag, long timestamp)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OUR_Set<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            set = set.Add(value, tag, timestamp);

            _repository.PersistAdds(set.Adds);
        }

        public void Update(T value, Guid tag, long timestamp)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OUR_Set<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            set = set.Update(value, tag, timestamp);

            _repository.PersistAdds(set.Adds);
        }

        public void Remove(T value, IEnumerable<Guid> tags, long timestamp)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OUR_Set<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            foreach (var tag in tags)
            {
                set = set.Remove(value, tag, timestamp);
            }

            _repository.PersistRemoves(set.Removes);
        }

        public bool Lookup(T value)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OUR_Set<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            var lookup = set.Lookup(value);

            return lookup;
        }
    }
}