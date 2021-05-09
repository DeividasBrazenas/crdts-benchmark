using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Commutative.TwoPhase;

namespace CRDT.Application.Commutative.Set
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

        public void DownstreamAdd(T value)
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

        public void DownstreamRemove(T value)
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

        public bool Lookup(T value)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new P_Set<T>(existingAdds, existingRemoves);

            var lookup = set.Lookup(value);

            return lookup;
        }
    }
}