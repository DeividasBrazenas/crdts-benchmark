using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Convergent.LastWriterWins;
using CRDT.Sets.Entities;

namespace CRDT.Application.Convergent.Set
{
    public class LWW_SetWithVCService<T> where T : DistributedEntity
    {
        private readonly ILWW_SetWithVCRepository<T> _repository;
        private readonly object _lockObject = new();

        public LWW_SetWithVCService(ILWW_SetWithVCRepository<T> repository)
        {
            _repository = repository;
        }

        public void LocalAssign(T value, VectorClock vectorClock)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new LWW_SetWithVC<T>(existingAdds, existingRemoves);

                set = set.Add(value, vectorClock);

                _repository.PersistAdds(set.Adds);
            }
        }

        public void LocalRemove(T value, VectorClock vectorClock)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new LWW_SetWithVC<T>(existingAdds, existingRemoves);

                set = set.Remove(value, vectorClock);

                _repository.PersistRemoves(set.Removes);
            }
        }
        public void Merge(ImmutableHashSet<LWW_SetWithVCElement<T>> adds, ImmutableHashSet<LWW_SetWithVCElement<T>> removes)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new LWW_SetWithVC<T>(existingAdds, existingRemoves);

                set = set.Merge(adds, removes);

                _repository.PersistAdds(set.Adds);
                _repository.PersistRemoves(set.Removes);
            }
        }

        public bool Lookup(T value)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new LWW_SetWithVC<T>(existingAdds, existingRemoves);

            var lookup = set.Lookup(value);

            return lookup;
        }

        public (ImmutableHashSet<LWW_SetWithVCElement<T>>, ImmutableHashSet<LWW_SetWithVCElement<T>>) State => (_repository.GetAdds(), _repository.GetRemoves());
    }
}