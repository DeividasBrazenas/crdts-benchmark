using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Commutative.LastWriterWins;

namespace CRDT.Application.Commutative.Set
{
    public class LWW_SetService<T> where T : DistributedEntity
    {
        private readonly ILWW_SetRepository<T> _repository;
        private readonly object _lockObject = new();

        public LWW_SetService(ILWW_SetRepository<T> repository)
        {
            _repository = repository;
        }

        public void LocalAssign(T value, long timestamp)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new LWW_Set<T>(existingAdds, existingRemoves);

                set = set.Assign(value, timestamp);

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

        public void DownstreamAssign(T value, long timestamp)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new LWW_Set<T>(existingAdds, existingRemoves);

                set = set.Assign(value, timestamp);

                _repository.PersistAdds(set.Adds);
            }
        }

        public void DownstreamRemove(T value, long timestamp)
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

        public bool Lookup(T value)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new LWW_Set<T>(existingAdds, existingRemoves);

            var lookup = set.Lookup(value);

            return lookup;
        }
    }
}