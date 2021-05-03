using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Convergent
{
    public sealed class U_Set<T> : U_SetBase<T> where T : DistributedEntity
    {
        public U_Set()
        {
        }

        public U_Set(IImmutableSet<U_SetElement<T>> elements) : base(elements)
        {
        }

        public U_Set<T> Merge(IImmutableSet<U_SetElement<T>> elements)
        {
            var union = Elements.Union(elements);

            var validElements = new HashSet<U_SetElement<T>>();

            foreach (var element in union)
            {
                if (!element.Removed)
                {
                    if(union.Any(e => Equals(element.Value, e.Value) && e.Removed))
                    {
                        continue;
                    }
                }

                validElements.Add(element);
            }

            return new(validElements.ToImmutableHashSet());
        }
    }
}