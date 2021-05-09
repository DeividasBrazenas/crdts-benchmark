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
    public class OUR_OptimizedSetWithVCService<T> where T : DistributedEntity
    {
        private readonly IOUR_OptimizedSetWithVCRepository<T> _repository;
        private readonly object _lockObject = new();

        public OUR_OptimizedSetWithVCService(IOUR_OptimizedSetWithVCRepository<T> repository)
        {
            _repository = repository;
        }

        public void LocalAdd(T value, Guid tag, VectorClock vectorClock)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new OUR_OptimizedSetWithVC<T>(existingElements);

                set = set.Add(value, tag, vectorClock);

                _repository.PersistElements(set.Elements);
            }
        }

        public void LocalUpdate(T value, IEnumerable<Guid> tags, VectorClock vectorClock)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new OUR_OptimizedSetWithVC<T>(existingElements);

                foreach (var tag in tags)
                {
                    set = set.Update(value, tag, vectorClock);
                }

                _repository.PersistElements(set.Elements);
            }
        }

        public void LocalRemove(T value, IEnumerable<Guid> tags, VectorClock vectorClock)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new OUR_OptimizedSetWithVC<T>(existingElements);

                foreach (var tag in tags)
                {
                    set = set.Remove(value, tag, vectorClock);
                }

                _repository.PersistElements(set.Elements);
            }
        }

        public void DownstreamAdd(T value, Guid tag, VectorClock vectorClock)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new OUR_OptimizedSetWithVC<T>(existingElements);

                set = set.Add(value, tag, vectorClock);

                _repository.PersistElements(set.Elements);
            }
        }

        public void DownstreamUpdate(T value, IEnumerable<Guid> tags, VectorClock vectorClock)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new OUR_OptimizedSetWithVC<T>(existingElements);

                foreach (var tag in tags)
                {
                    set = set.Update(value, tag, vectorClock);
                }

                _repository.PersistElements(set.Elements);
            }
        }

        public void DownstreamRemove(T value, IEnumerable<Guid> tags, VectorClock vectorClock)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new OUR_OptimizedSetWithVC<T>(existingElements);

                foreach (var tag in tags)
                {
                    set = set.Remove(value, tag, vectorClock);
                }

                _repository.PersistElements(set.Elements);
            }
        }

        public bool Lookup(T value)
        {
            var existingElements = _repository.GetElements();

            var set = new OUR_OptimizedSetWithVC<T>(existingElements);

            var lookup = set.Lookup(value);

            return lookup;
        }

        public List<Guid> GetTags(Guid id)
        {
            var existingElements = _repository.GetElements();

            return existingElements.Where(e => e.Tag == id && !e.Removed).Select(e => e.Tag).ToList();
        }
    }
}