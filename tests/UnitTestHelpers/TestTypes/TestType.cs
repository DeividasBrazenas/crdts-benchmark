using System;
using Abstractions.Entities;

namespace UnitTestHelpers.TestTypes
{
    public class TestType : DistributedEntity
    {
        public string StringValue { get; set; }

        public int IntValue { get; set; }

        public decimal DecimalValue { get; set; }

        public long? NullableLongValue { get; set; }

        public Guid? GuidValue { get; set; }

        public InnerTestType ObjectValue { get; set; }


        public TestType(Guid id) : base(id)
        {
        }
    }
}