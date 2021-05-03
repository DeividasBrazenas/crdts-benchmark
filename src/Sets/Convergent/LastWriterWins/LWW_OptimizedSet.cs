using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Convergent.LastWriterWins
{
    public sealed class LWW_OptimizedSet<T> : LWW_OptimizedSetBase<T> where T : DistributedEntity
    {
        public LWW_OptimizedSet()
        {
        }

        public LWW_OptimizedSet(IImmutableSet<LWW_OptimizedSetElement<T>> elements)
            : base(elements)
        {
        }

        public LWW_OptimizedSet<T> Merge(IImmutableSet<LWW_OptimizedSetElement<T>> elements)
        {
            var union = Elements.Union(elements);

            var validElements = new HashSet<LWW_OptimizedSetElement<T>>();

            foreach (var element in union)
            {
                if (!element.Removed)
                {
                    if (union.Any(e => Equals(element.Value, e.Value) && e.Removed && e.Timestamp > element.Timestamp))
                    {
                        continue;
                    }
                }
                else
                {
                    if (union.Any(e => Equals(element.Value, e.Value) && !e.Removed && e.Timestamp > element.Timestamp))
                    {
                        continue;
                    }
                }

                validElements.Add(element);
            }

            var filteredElements = validElements
                .Where(a => !validElements.Any(oa => a.Value.Id == oa.Value.Id && a.Timestamp < oa.Timestamp));

            return new(filteredElements.ToImmutableHashSet());
        }
    }
}