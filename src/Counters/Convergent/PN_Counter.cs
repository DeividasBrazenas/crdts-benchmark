using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Counters.Bases;
using CRDT.Counters.Entities;

namespace CRDT.Counters.Convergent
{
    public class PN_Counter : PN_CounterBase
    {
        public PN_Counter(IImmutableSet<CounterElement> additions, IImmutableSet<CounterElement> subtractions) 
            : base(additions, subtractions)
        {
        }

        public PN_Counter Add(int value, Guid nodeId)
        {
            var element = Additions.FirstOrDefault(e => e.Node.Id == nodeId);

            if (element is null)
            {
                element = new CounterElement(value, nodeId);
            }
            else
            {
                element.Add(value);
            }

            var additions = Additions.Where(e => e.Node.Id != nodeId).ToList();
            additions.Add(element);

            return new PN_Counter(additions.ToImmutableHashSet(), Subtractions);
        }

        public PN_Counter Subtract(int value, Guid nodeId)
        {
            var element = Subtractions.FirstOrDefault(e => e.Node.Id == nodeId);

            if (element is null)
            {
                element = new CounterElement(value, nodeId);
            }
            else
            {
                element.Add(Math.Abs(value));
            }

            var subtractions = Subtractions.Where(e => e.Node.Id != nodeId).ToList();
            subtractions.Add(element);

            return new PN_Counter(Additions, subtractions.ToImmutableHashSet());
        }

        public PN_Counter Merge(IImmutableSet<CounterElement> additions, IImmutableSet<CounterElement> subtractions)
        {
            var mergedAdditions = MergeElements(Additions, additions);
            var mergedSubtractions = MergeElements(Subtractions, subtractions);

            return new PN_Counter(mergedAdditions, mergedSubtractions);
        }

        private IImmutableSet<CounterElement> MergeElements(IImmutableSet<CounterElement> firstSet, IImmutableSet<CounterElement> secondSet)
        {
            var commonNodes = firstSet.Where(e => secondSet.Any(el => Equals(e.Node, el.Node))).Select(s => s.Node);
            var firstSetElements = firstSet.Where(e => commonNodes.All(c => e.Node.Id != c.Id));
            var secondSetElements = secondSet.Where(e => commonNodes.All(c => e.Node.Id != c.Id));

            var mergedElements = new HashSet<CounterElement>();

            foreach (var node in commonNodes)
            {
                var firstSetElement = firstSet.First(e => Equals(e.Node, node));
                var secondSetElement = secondSet.First(e => Equals(e.Node, node));

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

            return mergedElements.ToImmutableHashSet();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Additions;
            yield return Subtractions;
            yield return Sum;
        }
    }
}