using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Core.Cluster;
using CRDT.Sets.Commutative;
using CRDT.Sets.Entities;

namespace CRDT.Application.Commutative
{
    public class OR_OptimizedSetService<T> where T : DistributedEntity
    {
        private readonly IOR_OptimizedSetRepository<T> _repository;

        public OR_OptimizedSetService(IOR_OptimizedSetRepository<T> repository)
        {
            _repository = repository;
        }

        public void Add(T value, Node node)
        {
            var existingElements = _repository.GetElements();

            var set = new OR_OptimizedSet<T>(existingElements.ToImmutableHashSet());

            set = set.Add(value, node.Id);

            _repository.PersistElements(set.Elements);
        }

        public void Remove(T value, IEnumerable<Guid> tags)
        {
            var existingElements = _repository.GetElements();

            var set = new OR_OptimizedSet<T>(existingElements.ToImmutableHashSet());

            foreach (var tag in tags)
            {
                set = set.Remove(value, tag);
            }

            _repository.PersistElements(set.Elements);
        }

        public bool Lookup(T value)
        {
            var existingElements = _repository.GetElements();

            var set = new OR_OptimizedSet<T>(existingElements.ToImmutableHashSet());

            var lookup = set.Lookup(value);

            return lookup;
        }
    }
}