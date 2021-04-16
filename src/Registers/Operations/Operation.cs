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

        public Operation(Guid elementId, JToken value, long timestamp, Node updatedBy) 
            : base(elementId, value, updatedBy)
        {
            Timestamp = new Timestamp(timestamp);
        }

        public static Operation Parse(string valueJson, long timestamp, Node updatedBy)
        {
            var jToken = JToken.Parse(valueJson);
            var idToken = jToken["Id"];

            return idToken is null ? null : new Operation(idToken.ToObject<Guid>(), jToken, timestamp, updatedBy);
        }
    }
}