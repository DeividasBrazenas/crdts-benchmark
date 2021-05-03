using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Commutative.ObservedUpdatedRemoved;

namespace CRDT.Application.Commutative.Set
{
    public class OUR_OptimizedSetWithVCService<T> where T : DistributedEntity
    {
        private readonly IOUR_OptimizedSetWithVCRepository<T> _repository;

        public OUR_OptimizedSetWithVCService(IOUR_OptimizedSetWithVCRepository<T> repository)
        {
            _repository = repository;
        }

        public void Add(T value, Guid tag, VectorClock vectorClock)
        {
            var existingElements = _repository.GetElements();

            var set = new OUR_OptimizedSetWithVC<T>(existingElements.ToImmutableHashSet());

            set = set.Add(value, tag, vectorClock);

            _repository.PersistElements(set.Elements);
        }

        public void Update(T value, Guid tag, VectorClock vectorClock)
        {
            var existingElements = _repository.GetElements();

            var set = new OUR_OptimizedSetWithVC<T>(existingElements.ToImmutableHashSet());

            set = set.Update(value, tag, vectorClock);

            _repository.PersistElements(set.Elements);
        }

        public void Remove(T value, IEnumerable<Guid> tags, VectorClock vectorClock)
        {
            var existingElements = _repository.GetElements();

            var set = new OUR_OptimizedSetWithVC<T>(existingElements.ToImmutableHashSet());

            foreach (var tag in tags)
            {
                set = set.Remove(value, tag, vectorClock);
            }

            _repository.PersistElements(set.Elements);
        }

        public bool Lookup(T value)
        {
            var existingElements = _repository.GetElements();

            var set = new OUR_OptimizedSetWithVC<T>(existingElements.ToImmutableHashSet());

            var lookup = set.Lookup(value);

            return lookup;
        }
    }
}