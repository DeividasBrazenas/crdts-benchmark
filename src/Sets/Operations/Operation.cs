using Newtonsoft.Json.Linq;

namespace CRDT.Sets.Operations
{
    public sealed class Operation
    {
        public JToken Value { get; }

        public Operation(JToken value)
        {
            Value = value;
        }
    }
}