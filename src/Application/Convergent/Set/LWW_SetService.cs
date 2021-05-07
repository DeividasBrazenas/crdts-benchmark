using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Convergent.LastWriterWins;
using CRDT.Sets.Entities;

namespace CRDT.Application.Convergent.Set
{
    public class LWW_SetService<T> where T : DistributedEntity
    {
        private readonly ILWW_SetRepository<T> _repository;
        private readonly object _lockObject = new();

        public LWW_SetService(ILWW_SetRepository<T> repository)
        {
            _repository = repository;
        }

        public void LocalAdd(T value, long timestamp)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new LWW_Set<T>(existingAdds, existingRemoves);

                set = set.Add(value, timestamp);

                _repository.PersistAdds(set.Adds);
            }
        }

        public void LocalRemove(T value, long timestamp)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new LWW_Set<T>(existingAdds, existingRemoves);

                set = set.Remove(value, timestamp);

                _repository.PersistRemoves(set.Removes);
            }
        }
        public void Merge(ImmutableHashSet<LWW_SetElement<T>> adds, ImmutableHashSet<LWW_SetElement<T>> removes)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new LWW_Set<T>(existingAdds, existingRemoves);

                set = set.Merge(adds, removes);

                _repository.PersistAdds(set.Adds);
                _repository.PersistRemoves(set.Removes);
            }
        }

        public bool Lookup(T value)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new LWW_Set<T>(existingAdds, existingRemoves);

            var lookup = set.Lookup(value);

            return lookup;
        }

        public (ImmutableHashSet<LWW_SetElement<T>>, ImmutableHashSet<LWW_SetElement<T>>) State =>
            (_repository.GetAdds(), _repository.GetRemoves());
    }
}