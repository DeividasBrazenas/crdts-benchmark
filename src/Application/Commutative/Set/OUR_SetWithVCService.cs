using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Commutative.ObservedUpdatedRemoved;

namespace CRDT.Application.Commutative.Set
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

        public void DownstreamAdd(T value, Guid tag, VectorClock vectorClock)
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

        public void DownstreamUpdate(T value, IEnumerable<Guid> tags, VectorClock vectorClock)
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

        public void DownstreamRemove(T value, IEnumerable<Guid> tags, VectorClock vectorClock)
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

        public bool Lookup(T value)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OUR_SetWithVC<T>(existingAdds, existingRemoves);

            var lookup = set.Lookup(value);

            return lookup;
        }

        public List<Guid> GetTags(Guid id)
        {
            var adds = _repository.GetAdds(id);
            var removes = _repository.GetRemoves(id);

            return adds.Except(removes).Select(a => a.Tag).ToList();
        }
    }
}