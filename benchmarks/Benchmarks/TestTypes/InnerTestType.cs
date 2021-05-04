using System.Collections.Generic;
using CRDT.Core.Abstractions;

namespace Benchmarks.TestTypes
{
    public class InnerTestType : ValueObject
    {
        public string StringValue { get; set; }

        public int IntValue { get; set; }

        public decimal DecimalValue { get; set; }

        public long? NullableLongValue { get; set; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return StringValue;
            yield return IntValue;
            yield return DecimalValue;
            yield return NullableLongValue;
        }
    }
}