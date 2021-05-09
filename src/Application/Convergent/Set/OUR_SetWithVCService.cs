using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Convergent.ObservedUpdatedRemoved;
using CRDT.Sets.Entities;

namespace CRDT.Application.Convergent.Set
{
    public class OUR_SetWithVCService<T> where T : DistributedEntity
    {
        private readonly IOUR_SetWithVCRepository<T> _repository;
        private readonly object _lockObject = new();
        public OUR_SetWithVCService(IOUR_SetWithVCRepository<T> repository)
        {
            _repository = repository;
        }

        public void LocalAdd(T value, Guid tag, VectorClock vectorClock)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new OUR_SetWithVC<T>(existingAdds, existingRemoves);

                set = set.Add(value, tag, vectorClock);

                _repository.PersistAdds(set.Adds);
            }
        }

        public void LocalUpdate(T value, IEnumerable<Guid> tags, VectorClock vectorClock)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new OUR_SetWithVC<T>(existingAdds, existingRemoves);

                foreach (var tag in tags)
                {
                    set = set.Update(value, tag, vectorClock);
                }

                _repository.PersistAdds(set.Adds);
            }
        }

        public void LocalRemove(T value, IEnumerable<Guid> tags, VectorClock vectorClock)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new OUR_SetWithVC<T>(existingAdds, existingRemoves);

                foreach (var tag in tags)
                {
                    set = set.Remove(value, tag, vectorClock);
                }

                _repository.PersistRemoves(set.Removes);
            }
        }
        public void Merge(ImmutableHashSet<OUR_SetWithVCElement<T>> adds, ImmutableHashSet<OUR_SetWithVCElement<T>> removes)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new OUR_SetWithVC<T>(existingAdds, existingRemoves);

                set = set.Merge(adds, removes);

                _repository.PersistAdds(set.Adds);
                _repository.PersistRemoves(set.Removes);
            }
        }

        public bool Lookup(T value)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OUR_SetWithVC<T>(existingAdds, existingRemoves);

            var lookup = set.Lookup(value);

            return lookup;
        }

        public (ImmutableHashSet<OUR_SetWithVCElement<T>>, ImmutableHashSet<OUR_SetWithVCElement<T>>) State =>
            (_repository.GetAdds(), _repository.GetRemoves());

        public List<Guid> GetTags(Guid id)
        {
            var adds = _repository.GetAdds(id);
            var removes = _repository.GetRemoves(id);

            return adds.Except(removes).Select(a => a.Tag).ToList();
        }
    }
}