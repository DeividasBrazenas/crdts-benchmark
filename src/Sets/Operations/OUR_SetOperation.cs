using CRDT.Core.DistributedTime;
using Newtonsoft.Json.Linq;

namespace CRDT.Sets.Operations
{
    public sealed class OUR_SetOperation
    {
        public JToken Value { get; }

        public Timestamp Timestamp { get; }

        public OUR_SetOperation(JToken value, long timestamp)
        {
            Value = value;
            Timestamp = new Timestamp(timestamp);
        }
    }
}