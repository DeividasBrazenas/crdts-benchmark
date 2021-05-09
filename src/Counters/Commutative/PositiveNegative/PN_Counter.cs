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
        public PN_Counter(ImmutableHashSet<CounterElement> additions, ImmutableHashSet<CounterElement> subtractions) 
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

            var additions = Additions.Where(e => e.Node.Id != nodeId).ToImmutableHashSet();
            additions = additions.Add(element);

            return new PN_Counter(additions, Subtractions);
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

            var subtractions = Subtractions.Where(e => e.Node.Id != nodeId).ToImmutableHashSet();
            subtractions = subtractions.Add(element);

            return new PN_Counter(Additions, subtractions);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Additions;
            yield return Subtractions;
            yield return Sum;
        }
    }
}