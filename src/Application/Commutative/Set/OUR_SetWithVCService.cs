using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Commutative.ObservedUpdatedRemoved;

namespace CRDT.Application.Commutative.Set
{
    public class OUR_SetWithVCService<T> where T : DistributedEntity
    {
        private readonly IOUR_SetWithVCRepository<T> _repository;

        public OUR_SetWithVCService(IOUR_SetWithVCRepository<T> repository)
        {
            _repository = repository;
        }

        public void Add(T value, Guid tag, VectorClock vectorClock)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OUR_SetWithVC<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            set = set.Add(value, tag, vectorClock);

            _repository.PersistAdds(set.Adds);
        }

        public void Update(T value, Guid tag, VectorClock vectorClock)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OUR_SetWithVC<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            set = set.Update(value, tag, vectorClock);

            _repository.PersistAdds(set.Adds);
        }

        public void Remove(T value, IEnumerable<Guid> tags, VectorClock vectorClock)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OUR_SetWithVC<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            foreach (var tag in tags)
            {
                set = set.Remove(value, tag, vectorClock);
            }

            _repository.PersistRemoves(set.Removes);
        }

        public bool Lookup(T value)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OUR_SetWithVC<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            var lookup = set.Lookup(value);

            return lookup;
        }
    }
}