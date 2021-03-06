using System;
using System.Collections.Generic;
using CRDT.Core.Abstractions;

namespace CRDT.Core.Cluster
{
    public class Node : ValueObject, IComparable<Node>
    {
        public Guid Id { get; }

        public Node()
        {
            Id = Guid.NewGuid();
        }

        public Node(Guid id)
        {
            Id = id;
        }

        public static List<Node> CreateNodes(int count)
        {
            var nodes = new List<Node>();

            for (var i = 0; i < count; i++)
            {
                nodes.Add(new Node());
            }

            return nodes;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }

        public static bool operator <(Node left, Node right)
            => left.Id.CompareTo(right.Id) < 0;

        public static bool operator >(Node left, Node right)
            => left.Id.CompareTo(right.Id) > 0;

        public int CompareTo(Node other) => Comparer<Guid>.Default.Compare(Id, other.Id);
    }
}