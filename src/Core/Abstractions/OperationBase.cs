using CRDT.Core.Cluster;
using Newtonsoft.Json.Linq;

namespace CRDT.Core.Abstractions
{
    public abstract class OperationBase
    {
        public JToken Value { get; protected set; }

        public Node UpdatedBy { get; }

        protected OperationBase(JToken value, Node updatedBy)
        {
            Value = value;
            UpdatedBy = updatedBy;
        }
    }
}