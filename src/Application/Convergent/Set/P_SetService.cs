using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Convergent.TwoPhase;

namespace CRDT.Application.Convergent.Set
{
    public class P_SetService<T> where T : DistributedEntity
    {
        private readonly IP_SetRepository<T> _repository;
        private readonly object _lockObject = new();

        public P_SetService(IP_SetRepository<T> repository)
        {
            _repository = repository;
        }

        public void LocalAdd(T value)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new P_Set<T>(existingAdds, existingRemoves);

                set = set.Add(value);

                _repository.PersistAdds(set.Adds);
            }
        }

        public void LocalRemove(T value)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new P_Set<T>(existingAdds, existingRemoves);

                set = set.Remove(value);

                _repository.PersistRemoves(set.Removes);
            }
        }

        public void Merge(ImmutableHashSet<T> adds, ImmutableHashSet<T> removes)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new P_Set<T>(existingAdds, existingRemoves);

                set = set.Merge(adds, removes);

                _repository.PersistAdds(set.Adds);
                _repository.PersistRemoves(set.Removes);
            }
        }

        public bool Lookup(T value)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new P_Set<T>(existingAdds, existingRemoves);

            var lookup = set.Lookup(value);

            return lookup;
        }

        public (ImmutableHashSet<T>, ImmutableHashSet<T>) State => (_repository.GetAdds(), _repository.GetRemoves());
    }
}