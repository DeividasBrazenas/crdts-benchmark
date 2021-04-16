using System;
using CRDT.Core.Abstractions;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using Newtonsoft.Json.Linq;

namespace CRDT.Registers.Operations
{
    public sealed class Operation : OperationBase
    {
        public Timestamp Timestamp { get; }

        public Operation(JToken value, long timestamp, Node updatedBy) 
            : base(value, updatedBy)
        {
            Timestamp = new Timestamp(timestamp);
        }
    }
}