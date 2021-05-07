using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Convergent.ObservedRemoved;
using CRDT.Sets.Entities;

namespace CRDT.Application.Convergent.Set
{
    public class OR_SetService<T> where T : DistributedEntity
    {
        private readonly IOR_SetRepository<T> _repository;
        private readonly object _lockObject = new();

        public OR_SetService(IOR_SetRepository<T> repository)
        {
            _repository = repository;
        }

        public void LocalAdd(T value, Guid tag)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new OR_Set<T>(existingAdds, existingRemoves);

                set = set.Add(value, tag);

                _repository.PersistAdds(set.Adds);
            }
        }

        public void LocalRemove(T value, IEnumerable<Guid> tags)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new OR_Set<T>(existingAdds, existingRemoves);

                foreach (var tag in tags)
                {
                    set = set.Remove(value, tag);
                }

                _repository.PersistRemoves(set.Removes);
            }
        }
        public void Merge(ImmutableHashSet<OR_SetElement<T>> adds, ImmutableHashSet<OR_SetElement<T>> removes)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new OR_Set<T>(existingAdds, existingRemoves);

                set = set.Merge(adds, removes);

                _repository.PersistAdds(set.Adds);
                _repository.PersistRemoves(set.Removes);
            }
        }

        public bool Lookup(T value)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OR_Set<T>(existingAdds, existingRemoves);

            var lookup = set.Lookup(value);

            return lookup;
        }

        public (ImmutableHashSet<OR_SetElement<T>>, ImmutableHashSet<OR_SetElement<T>>) State =>
            (_repository.GetAdds(), _repository.GetRemoves());


        public List<Guid> GetTags(T value)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new Sets.Commutative.ObservedRemoved.OR_Set<T>(existingAdds, existingRemoves);

            return set.Elements.Where(e => Equals(e.Value, value)).Select(e => e.Tag).ToList();
        }
    }
}