using System;
using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Registers.Entities;
using Newtonsoft.Json.Linq;

namespace CRDT.Registers.Commutative.LastWriterWins
{
    public sealed class LWW_RegisterWithVC<T> : Bases.LWW_RegisterWithVCBase<T>
        where T : DistributedEntity
    {
        public LWW_RegisterWithVC(LWW_RegisterWithVCElement<T> element) : base(element)
        {
        }

        public LWW_RegisterWithVC<T> Assign(JToken value, VectorClock vectorClock)
        {
            if (Element.VectorClock < vectorClock)
            {
                return AssignValue(value, vectorClock);
            }

            return this;
        }

        private LWW_RegisterWithVC<T> AssignValue(JToken value, VectorClock vectorClock)
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

            return new LWW_RegisterWithVC<T>(new LWW_RegisterWithVCElement<T>(newValue, vectorClock));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Element;
        }
    }
}