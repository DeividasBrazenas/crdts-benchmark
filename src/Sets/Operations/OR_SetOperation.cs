using System;
using Newtonsoft.Json.Linq;

namespace CRDT.Sets.Operations
{
    public sealed class OR_SetOperation
    {
        public JToken Value { get; }

        public Guid Tag { get; }

        public OR_SetOperation(JToken value, Guid tag)
        {
            Value = value;
            Tag = tag;
        }
    }
}