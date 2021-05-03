using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Counters.Bases;
using CRDT.Counters.Entities;

namespace CRDT.Counters.Commutative.GrowOnly
{
    public class G_Counter : G_CounterBase
    {
        public G_Counter(IImmutableSet<CounterElement> elements) : base(elements)
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

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Elements;
        }
    }
}