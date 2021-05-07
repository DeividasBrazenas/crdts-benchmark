using System;
using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Registers.Entities;
using Newtonsoft.Json.Linq;

namespace CRDT.Registers.Commutative.LastWriterWins
{
    public sealed class LWW_Register<T> : Bases.LWW_RegisterBase<T>
        where T : DistributedEntity
    {
        public LWW_Register(LWW_RegisterElement<T> element) : base(element)
        {
        }

        public LWW_Register<T> Assign(JToken value, long timestamp)
        {
            if (Element.Timestamp < timestamp)
            {
                return AssignValue(value, timestamp);
            }

            return this;
        }

        private LWW_Register<T> AssignValue(JToken value, long timestamp)
        {
            var currentValue = JObject.FromObject(Element.Value);

            currentValue.Merge(value, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Merge,
                PropertyNameComparison = StringComparison.InvariantCultureIgnoreCase
            });

            var newValue = currentValue.ToObject<T>();

            if (Element.Value.Equals(newValue))
            {
                return this;
            }

            return new LWW_Register<T>(new LWW_RegisterElement<T>(newValue, timestamp));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Element;
        }
    }
}