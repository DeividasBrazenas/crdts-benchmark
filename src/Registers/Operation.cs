using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using Newtonsoft.Json.Linq;

namespace CRDT.Registers
{
    public sealed class Operation
    {
        public Timestamp Timestamp { get; }

        public Node UpdatedBy { get; }

        public JToken Value { get; }

        public Operation(long timestamp, Node updatedBy, JToken value)
        {
            Timestamp = new Timestamp(timestamp);
            UpdatedBy = updatedBy;
            Value = value;
        }
    }
}