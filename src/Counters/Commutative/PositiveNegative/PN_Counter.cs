using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Counters.Bases;
using CRDT.Counters.Entities;

namespace CRDT.Counters.Commutative.PositiveNegative
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

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Additions;
            yield return Subtractions;
            yield return Sum;
        }
    }
}