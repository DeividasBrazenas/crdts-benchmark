using System;
using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Core.Cluster;

namespace CRDT.Counters.Entities
{
    public class CounterElement : ValueObject
    {
        public int Value { get; private set; }

        public Node Node { get; }

        public CounterElement(int value, Guid nodeId)
        {
            Value = value;
            Node = new Node(nodeId);
        }

        public void Add(int value)
        {
            Value += value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return Node;
        }
    }
}