using System;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using Newtonsoft.Json.Linq;

namespace CRDT.Core.Abstractions
{
    public abstract class OperationBase
    {
        public Guid OperationId { get; }

        public JToken Value { get; protected set; }

        public Node UpdatedBy { get; }

        protected OperationBase(JToken value, Node updatedBy)
        {
            OperationId = Guid.NewGuid();
            Value = value;
            UpdatedBy = updatedBy;
        }
    }
}