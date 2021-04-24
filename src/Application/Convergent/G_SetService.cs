using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Application.Entities;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Core.Cluster;
using CRDT.Sets.Convergent;

namespace CRDT.Application.Convergent
{
    public class G_SetService<T> where T : DistributedEntity
    {
        private readonly INewRepository<T> _repository;

        public G_SetService(INewRepository<T> repository)
        {
            _repository = repository;
        }

        public void Add(T value)
        {
            var existingEntities = _repository.GetValues();

            var set = new G_Set<T>(existingEntities.ToImmutableHashSet());

            set = set.Add(value);

            _repository.AddValues(set.Values);
        }

        public void Merge(IEnumerable<T> values)
        {
            var existingEntities = _repository.GetValues();

            var set = new G_Set<T>(existingEntities.ToImmutableHashSet());

            set = set.Merge(values.ToImmutableHashSet());

            _repository.AddValues(set.Values);
        }

        public bool Lookup(T value)
        {
            var existingEntities = _repository.GetValues();

            var set = new G_Set<T>(existingEntities.ToImmutableHashSet());

            var lookup = set.Lookup(value);

            return lookup;
        }
    }
}