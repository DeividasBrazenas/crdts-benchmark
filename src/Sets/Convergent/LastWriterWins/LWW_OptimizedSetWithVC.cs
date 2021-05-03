using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Core.Abstractions;
using CRDT.Sets.Bases;
using CRDT.Sets.Entities;

namespace CRDT.Sets.Convergent.LastWriterWins
{
    public sealed class LWW_OptimizedSetWithVC<T> : LWW_OptimizedSetWithVCBase<T> where T : DistributedEntity
    {
        public LWW_OptimizedSetWithVC()
        {
        }

        public LWW_OptimizedSetWithVC(IImmutableSet<LWW_OptimizedSetWithVCElement<T>> elements)
            : base(elements)
        {
        }

        public LWW_OptimizedSetWithVC<T> Merge(IImmutableSet<LWW_OptimizedSetWithVCElement<T>> elements)
        {
            var union = Elements.Union(elements);

            var validElements = new HashSet<LWW_OptimizedSetWithVCElement<T>>();

            foreach (var element in union)
            {
                if (!element.Removed)
                {
                    if (union.Any(e => Equals(element.Value, e.Value) && e.Removed && e.VectorClock > element.VectorClock))
                    {
                        continue;
                    }
                }
                else
                {
                    if (union.Any(e => Equals(element.Value, e.Value) && !e.Removed && e.VectorClock > element.VectorClock))
                    {
                        continue;
                    }
                }

                validElements.Add(element);
            }

            var filteredElements = validElements
                .Where(a => !validElements.Any(oa => a.Value.Id == oa.Value.Id && a.VectorClock < oa.VectorClock));

            return new(filteredElements.ToImmutableHashSet());
        }
    }
}