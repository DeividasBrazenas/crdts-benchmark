using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Commutative.LastWriterWins;

namespace CRDT.Application.Commutative.Set
{
    public class LWW_SetWithVCService<T> where T : DistributedEntity
    {
        private readonly ILWW_SetWithVCRepository<T> _repository;

        public LWW_SetWithVCService(ILWW_SetWithVCRepository<T> repository)
        {
            _repository = repository;
        }

        public void Add(T value, VectorClock vectorClock)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new LWW_SetWithVC<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            set = set.Add(value, vectorClock);

            _repository.PersistAdds(set.Adds);
        }

        public void Remove(T value, VectorClock vectorClock)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new LWW_SetWithVC<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            set = set.Remove(value, vectorClock);

            _repository.PersistRemoves(set.Removes);
        }

        public bool Lookup(T value)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new LWW_SetWithVC<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            var lookup = set.Lookup(value);

            return lookup;
        }
    }
}