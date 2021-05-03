﻿using System;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Convergent
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

        public OR_Set<T> Merge(IImmutableSet<OR_SetElement<T>> adds, IImmutableSet<OR_SetElement<T>> removes)
        {
            var addsUnion = Adds.Union(adds);

            var removesUnion = Removes.Union(removes);

            var validRemoves = removesUnion.Where(r => addsUnion.Any(a => Equals(a, r)));

            return new(addsUnion.ToImmutableHashSet(), validRemoves.ToImmutableHashSet());
        }
    }
}