using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Commutative.ObservedRemoved;

namespace CRDT.Application.Commutative.Set
{
    public class OR_OptimizedSetService<T> where T : DistributedEntity
    {
        private readonly IOR_OptimizedSetRepository<T> _repository;
        private readonly object _lockObject = new();

        public OR_OptimizedSetService(IOR_OptimizedSetRepository<T> repository)
        {
            _repository = repository;
        }

        public void LocalAdd(T value, Guid tag)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new OR_OptimizedSet<T>(existingElements);

                set = set.Add(value, tag);

                _repository.PersistElements(set.Elements);
            }
        }

        public void LocalRemove(T value, IEnumerable<Guid> tags)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new OR_OptimizedSet<T>(existingElements);

                foreach (var tag in tags)
                {
                    set = set.Remove(value, tag);
                }

                _repository.PersistElements(set.Elements);
            }
        }

        public void DownstreamAdd(T value, Guid tag)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new OR_OptimizedSet<T>(existingElements);

                set = set.Add(value, tag);

                _repository.PersistElements(set.Elements);
            }
        }

        public void DownstreamRemove(T value, IEnumerable<Guid> tags)
        {
            lock (_lockObject)
            {
                var existingElements = _repository.GetElements();

                var set = new OR_OptimizedSet<T>(existingElements);

                foreach (var tag in tags)
                {
                    set = set.Remove(value, tag);
                }

                _repository.PersistElements(set.Elements);
            }
        }

        public bool Lookup(T value)
        {
            var existingElements = _repository.GetElements();

            var set = new OR_OptimizedSet<T>(existingElements);

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