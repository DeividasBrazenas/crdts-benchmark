using System;
using System.Collections.Generic;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Commutative.ObservedUpdatedRemoved;

namespace CRDT.Application.Commutative.Set
{
    public class OUR_OptimizedSetService<T> where T : DistributedEntity
    {
        private readonly IOUR_OptimizedSetRepository<T> _repository;
        private readonly object _lockObject = new();

        public OUR_OptimizedSetService(IOUR_OptimizedSetRepository<T> repository)
        {
            _repository = repository;
        }

        public void LocalAdd(T value, Guid tag, long timestamp)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new OUR_OptimizedSet<T>(existingElements);

                set = set.Add(value, tag, timestamp);

                _repository.PersistElements(set.Elements);
            }
        }

        public void LocalUpdate(T value, IEnumerable<Guid> tags, long timestamp)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new OUR_OptimizedSet<T>(existingElements);

                foreach (var tag in tags)
                {
                    set = set.Update(value, tag, timestamp);
                }

                _repository.PersistElements(set.Elements);
            }
        }

        public void LocalRemove(T value, IEnumerable<Guid> tags, long timestamp)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new OUR_OptimizedSet<T>(existingElements);

                foreach (var tag in tags)
                {
                    set = set.Remove(value, tag, timestamp);
                }

                _repository.PersistElements(set.Elements);
            }
        }

        public void DownstreamAdd(T value, Guid tag, long timestamp)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new OUR_OptimizedSet<T>(existingElements);

                set = set.Add(value, tag, timestamp);

                _repository.PersistElements(set.Elements);
            }
        }

        public void DownstreamUpdate(T value, IEnumerable<Guid> tags, long timestamp)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new OUR_OptimizedSet<T>(existingElements);

                foreach (var tag in tags)
                {
                    set = set.Update(value, tag, timestamp);
                }

                _repository.PersistElements(set.Elements);
            }
        }

        public void DownstreamRemove(T value, IEnumerable<Guid> tags, long timestamp)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new OUR_OptimizedSet<T>(existingElements);

                foreach (var tag in tags)
                {
                    set = set.Remove(value, tag, timestamp);
                }

                _repository.PersistElements(set.Elements);
            }
        }

        public bool Lookup(T value)
        {
            var existingElements = _repository.GetElements();

            var set = new OUR_OptimizedSet<T>(existingElements);

            var lookup = set.Lookup(value);

            return lookup;
        }

        public List<Guid> GetTags(Guid id)
        {
            var existingElements = _repository.GetElements(id);

            return existingElements.Where(e => !e.Removed).Select(e => e.Tag).ToList();
        }
    }
}