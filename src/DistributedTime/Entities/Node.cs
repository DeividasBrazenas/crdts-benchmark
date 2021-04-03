using System;
using System.Collections.Generic;
using System.Text;

namespace CRDT.DistributedTime.Entities
{
    public class Node : IComparable<Node>
    {
        public string Id { get; }
        private int ComputedHashValue;

        public Node(string id)
        {
            Id = id;
            ComputedHashValue = 42;

            unchecked
            {
                foreach (var c in Id)
                    ComputedHashValue *= ComputedHashValue * 21 + c; // Char byte value
            }
        }

        public static Node Create(string id)
        {
            return CreateNode(id);
        }

        private static Node CreateNode(string name)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = Encoding.UTF8.GetBytes(name);
            var hash = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();

            foreach (var t in hash) sb.Append(t.ToString("X2"));

            return new Node(sb.ToString());
        }

        public override int GetHashCode() => ComputedHashValue;

        public override bool Equals(object obj) => obj is Node that && Id.Equals(that.Id);

        public override string ToString() => Id;

        public int CompareTo(Node other) => Comparer<string>.Default.Compare(Id, other.Id);
    }
}