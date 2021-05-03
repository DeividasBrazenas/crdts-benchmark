using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Commutative;

namespace CRDT.Application.Commutative
{
    public class OUR_OptimizedSetService<T> where T : DistributedEntity
    {
        private readonly IOUR_OptimizedSetRepository<T> _repository;

        public OUR_OptimizedSetService(IOUR_OptimizedSetRepository<T> repository)
        {
            _repository = repository;
        }

        public void Add(T value, Guid tag, long timestamp)
        {
            var existingElements = _repository.GetElements();

            var set = new OUR_OptimizedSet<T>(existingElements.ToImmutableHashSet());

            set = set.Add(value, tag, timestamp);

            _repository.PersistElements(set.Elements);
        }

        public void Update(T value, Guid tag, long timestamp)
        {
            var existingElements = _repository.GetElements();

            var set = new OUR_OptimizedSet<T>(existingElements.ToImmutableHashSet());

            set = set.Update(value, tag, timestamp);

            _repository.PersistElements(set.Elements);
        }

        public void Remove(T value, IEnumerable<Guid> tags, long timestamp)
        {
            var existingElements = _repository.GetElements();

            var set = new OUR_OptimizedSet<T>(existingElements.ToImmutableHashSet());

            foreach (var tag in tags)
            {
                set = set.Remove(value, tag, timestamp);
            }

            _repository.PersistElements(set.Elements);
        }

        public bool Lookup(T value)
        {
            var existingElements = _repository.GetElements();

            var set = new OUR_OptimizedSet<T>(existingElements.ToImmutableHashSet());

            var lookup = set.Lookup(value);

            return lookup;
        }
    }
}