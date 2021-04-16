using System;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using Newtonsoft.Json.Linq;

namespace CRDT.Core.Abstractions
{
    public abstract class OperationBase
    {
        public Guid OperationId { get; }

        public Guid ElementId { get; protected set; }

        public JToken Value { get; protected set; }

        public Node UpdatedBy { get; }

        protected OperationBase(Guid elementId, JToken value, Node updatedBy)
        {
            OperationId = Guid.NewGuid();
            ElementId = elementId;
            Value = value;
            UpdatedBy = updatedBy;
        }
    }
}