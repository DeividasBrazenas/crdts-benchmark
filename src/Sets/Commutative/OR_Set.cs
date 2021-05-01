﻿using System;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Commutative
{
    public sealed class OR_Set<T> : OR_SetBase<T> where T : DistributedEntity
    {
        public OR_Set()
        {
        }

        public OR_Set(IImmutableSet<OR_SetElement<T>> adds, IImmutableSet<OR_SetElement<T>> removes)
            : base(adds, removes)
        {
        }

        public OR_Set<T> Add(T value, Guid tag) => new(Adds.Add(new OR_SetElement<T>(value, tag)), Removes);

        public OR_Set<T> Remove(T value, Guid tag)
        {
            if (Adds.Any(e => Equals(e.Value, value) && e.Tag == tag))
            {
                return new(Adds, Removes.Add(new OR_SetElement<T>(value, tag)));
            }

            return this;
        }
    }
}