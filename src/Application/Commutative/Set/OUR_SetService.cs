using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Commutative.ObservedUpdatedRemoved;

namespace CRDT.Application.Commutative.Set
{
    public class OUR_SetService<T> where T : DistributedEntity
    {
        private readonly IOUR_SetRepository<T> _repository;
        private readonly object _lockObject = new();

        public OUR_SetService(IOUR_SetRepository<T> repository)
        {
            _repository = repository;
        }

        public void LocalAdd(T value, Guid tag, long timestamp)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new OUR_Set<T>(existingAdds, existingRemoves);

                set = set.Add(value, tag, timestamp);

                _repository.PersistAdds(set.Adds);
            }
        }

        public void LocalUpdate(T value, Guid tag, long timestamp)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new OUR_Set<T>(existingAdds, existingRemoves);

                set = set.Update(value, tag, timestamp);

                _repository.PersistAdds(set.Adds);
            }
        }

        public void LocalRemove(T value, List<Guid> tags, long timestamp)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new OUR_Set<T>(existingAdds, existingRemoves);

                foreach (var tag in tags)
                {
                    set = set.Remove(value, tag, timestamp);
                }

                _repository.PersistRemoves(set.Removes);
            }
        }

        public void DownstreamAdd(T value, Guid tag, long timestamp)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new OUR_Set<T>(existingAdds, existingRemoves);

                set = set.Add(value, tag, timestamp);

                _repository.PersistAdds(set.Adds);
            }
        }

        public void DownstreamUpdate(T value, Guid tag, long timestamp)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new OUR_Set<T>(existingAdds, existingRemoves);

                set = set.Update(value, tag, timestamp);

                _repository.PersistAdds(set.Adds);
            }
        }

        public void DownstreamRemove(T value, List<Guid> tags, long timestamp)
        {
            lock (_lockObject)
            {
                var existingAdds = _repository.GetAdds();
                var existingRemoves = _repository.GetRemoves();

                var set = new OUR_Set<T>(existingAdds, existingRemoves);

                foreach (var tag in tags)
                {
                    set = set.Remove(value, tag, timestamp);
                }

                _repository.PersistRemoves(set.Removes);
            }
        }

        public bool Lookup(T value)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OUR_Set<T>(existingAdds, existingRemoves);

            var lookup = set.Lookup(value);

            return lookup;
        }

        public List<Guid> GetTags(T value)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OUR_Set<T>(existingAdds, existingRemoves);

            return set.Elements.Where(e => Equals(e.Value, value)).Select(e => e.Tag).ToList();
        }
    }
}