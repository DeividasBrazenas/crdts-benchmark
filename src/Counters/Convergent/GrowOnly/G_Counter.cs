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

        public G_Counter Merge(IImmutableSet<CounterElement> elements)
        {
            var commonNodes = Elements.Where(e => elements.Any(el => Equals(e.Node, el.Node))).Select(s => s.Node);
            var firstSetElements = Elements.Where(e => commonNodes.All(c => e.Node.Id != c.Id));
            var secondSetElements = elements.Where(e => commonNodes.All(c => e.Node.Id != c.Id));

            var mergedElements = new HashSet<CounterElement>();
            foreach (var node in commonNodes)
            {
                var firstSetElement = Elements.First(e => Equals(e.Node, node));
                var secondSetElement = elements.First(e => Equals(e.Node, node));

                mergedElements.Add(new CounterElement(Math.Max(firstSetElement.Value, secondSetElement.Value), node.Id));
            }

            foreach (var element in firstSetElements)
            {
                mergedElements.Add(element);
            }

            foreach (var element in secondSetElements)
            {
                mergedElements.Add(element);
            }

            return new G_Counter(mergedElements.ToImmutableHashSet());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Elements;
        }
    }
}