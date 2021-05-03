﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Commutative.ObservedRemoved;

namespace CRDT.Application.Commutative.Set
{
    public class OR_SetService<T> where T : DistributedEntity
    {
        private readonly IOR_SetRepository<T> _repository;

        public OR_SetService(IOR_SetRepository<T> repository)
        {
            _repository = repository;
        }

        public void Add(T value, Guid tag)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OR_Set<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            set = set.Add(value, tag);

            _repository.PersistAdds(set.Adds);
        }

        public void Remove(T value, IEnumerable<Guid> tags)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OR_Set<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            foreach (var tag in tags)
            {
                set = set.Remove(value, tag);
            }

            _repository.PersistRemoves(set.Removes);
        }

        public bool Lookup(T value)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OR_Set<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            var lookup = set.Lookup(value);

            return lookup;
        }
    }
}