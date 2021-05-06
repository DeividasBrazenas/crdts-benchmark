using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Counters.Bases;
using CRDT.Counters.Entities;

namespace CRDT.Counters.Convergent.GrowOnly
{
    public class G_Counter : G_CounterBase
    {
        public G_Counter(ImmutableHashSet<CounterElement> elements) : base(elements)
        {
        }

        public G_Counter Add(int value, Guid nodeId)
        {
            var element = Elements.FirstOrDefault(e => e.Node.Id == nodeId);

            if (element is null)
            {
                element = new CounterElement(value, nodeId);
            }
            else
            {
                element.Add(value);
            }

            var elements = Elements.Where(e => e.Node.Id != nodeId).ToList();
            elements.Add(element);

            return new G_Counter(elements.ToImmutableHashSet());
        }

        public G_Counter Merge(ImmutableHashSet<CounterElement> elements)
        {
            var union = Elements.Union(elements);
            var filteredElements = union.Where(u => !union.Any(e => Equals(u.Node, e.Node) && u.Value < e.Value));
                
            return new G_Counter(filteredElements.ToImmutableHashSet());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Elements;
        }
    }
}