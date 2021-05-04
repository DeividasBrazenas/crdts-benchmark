using System;
using System.Collections.Generic;
using CRDT.Core.Abstractions;
using Newtonsoft.Json;

namespace CRDT.UnitTestHelpers.TestTypes
{
    public class TestType : DistributedEntity
    {
        public string StringValue { get; set; }

        public int IntValue { get; set; }

        public decimal DecimalValue { get; set; }

        public long? NullableLongValue { get; set; }

        public Guid? GuidValue { get; set; }

        public int[] IntArray { get; set; }

        public List<long> LongList { get; set; }

        public InnerTestType ObjectValue { get; set; }

        public TestType(Guid id) : base(id)
        {
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return StringValue;
            yield return IntValue;
            yield return DecimalValue;
            yield return NullableLongValue;
            yield return GuidValue;
            yield return JsonConvert.SerializeObject(IntArray);
            yield return JsonConvert.SerializeObject(LongList);
            yield return ObjectValue;
        }
    }
}