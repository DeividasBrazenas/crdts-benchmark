using System;
using System.Collections.Generic;

namespace Cluster.Entities
{
    public class Node : IComparable<Node>
    {
        public Guid Id { get; }

        private readonly int _computedHashValue;

        public Node(Guid id)
        {
            Id = id;
            _computedHashValue = 42;

            unchecked
            {
                foreach (var c in Id.ToString())
                    _computedHashValue *= _computedHashValue * 21 + c; // Char byte value
            }
        }
        public override int GetHashCode() => _computedHashValue;

        public override bool Equals(object obj) => obj is Node that && Id.Equals(that.Id);

        public override string ToString() => Id.ToString();

        public static bool operator <(Node left, Node right)
            => left.Id.CompareTo(right.Id) < 0;

        public static bool operator >(Node left, Node right)
            => left.Id.CompareTo(right.Id) > 0;

        public int CompareTo(Node other) => Comparer<Guid>.Default.Compare(Id, other.Id);
    }
}