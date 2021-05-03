﻿using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Convergent.ObservedUpdatedRemoved;
using CRDT.Sets.Entities;

namespace CRDT.Application.Convergent.Set
{
    public class OUR_OptimizedSetService<T> where T : DistributedEntity
    {
        private readonly IOUR_OptimizedSetRepository<T> _repository;

        public OUR_OptimizedSetService(IOUR_OptimizedSetRepository<T> repository)
        {
            _repository = repository;
        }

        public void Merge(IEnumerable<OUR_OptimizedSetElement<T>> elements)
        {
            var existingElements = _repository.GetElements();

            var set = new OUR_OptimizedSet<T>(existingElements.ToImmutableHashSet());

            set = set.Merge(elements.ToImmutableHashSet());

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