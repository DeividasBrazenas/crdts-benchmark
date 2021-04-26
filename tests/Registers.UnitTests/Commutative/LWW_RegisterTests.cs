using System;
using AutoFixture.Xunit2;
using CRDT.Registers.Commutative;
using CRDT.Registers.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CRDT.Registers.UnitTests.Commutative
{
    public class LWW_RegisterTests
    {
        [Theory]
        [AutoData]
        public void Update_PrimitiveValues_SetsNewValues(TestType value,
            string stringValue, int intValue, decimal decimalValue, long longValue, Guid guidValue)
        {
            var lww = new LWW_Register<TestType>(new LWW_RegisterElement<TestType>(value, 0));

            var result = lww.Assign(JToken.Parse($"{{\"StringValue\": \"{stringValue}\"}}"), 1);
            result = result.Assign(JToken.Parse($"{{\"IntValue\": {intValue}}}"), 2);
            result = result.Assign(JToken.Parse($"{{\"DecimalValue\": {decimalValue}}}"), 3);
            result = result.Assign(JToken.Parse($"{{\"NullableLongValue\": {longValue}}}"), 4);
            result = result.Assign(JToken.Parse($"{{\"GuidValue\": \"{guidValue}\"}}"), 5);

            Assert.Equal(5, result.Element.Timestamp.Value);
            Assert.Equal(stringValue, result.Element.Value.StringValue);
            Assert.Equal(intValue, result.Element.Value.IntValue);
            Assert.Equal(decimalValue, result.Element.Value.DecimalValue);
            Assert.Equal(longValue, result.Element.Value.NullableLongValue);
            Assert.Equal(guidValue, result.Element.Value.GuidValue);
        }

        [Theory]
        [AutoData]
        public void Update_OperationsWithLowerTimestamp_DoNotTakeEffect(TestType value, 
            string stringValue, int intValue, decimal decimalValue, long longValue, Guid guidValue)
        {
            var lww = new LWW_Register<TestType>(new LWW_RegisterElement<TestType>(value, 10));

            var result = lww.Assign(JToken.Parse($"{{\"StringValue\": \"{stringValue}\"}}"), 1);
            result = result.Assign(JToken.Parse($"{{\"IntValue\": {intValue}}}"), 2);
            result = result.Assign(JToken.Parse($"{{\"DecimalValue\": {decimalValue}}}"), 3);
            result = result.Assign(JToken.Parse($"{{\"NullableLongValue\": {longValue}}}"), 4);
            result = result.Assign(JToken.Parse($"{{\"GuidValue\": \"{guidValue}\"}}"), 5);

            Assert.Equal(result, lww);
            Assert.Equal(value, result.Element.Value);
        }

        [Theory]
        [AutoData]
        public void Update_PrimitiveValuesWithMixedTimestamps_SetsNewValues(TestType value,
            string stringValue, int intValue, decimal decimalValue, long longValue, Guid guidValue)
        {
            var lww = new LWW_Register<TestType>(new LWW_RegisterElement<TestType>(value, 5));

            var result = lww.Assign(JToken.Parse($"{{\"StringValue\": \"{stringValue}\"}}"), 1);
            result = result.Assign(JToken.Parse($"{{\"IntValue\": {intValue}}}"), 2);
            result = result.Assign(JToken.Parse($"{{\"DecimalValue\": {decimalValue}}}"), 3);
            result = result.Assign(JToken.Parse($"{{\"NullableLongValue\": {longValue}}}"), 8);
            result = result.Assign(JToken.Parse($"{{\"GuidValue\": \"{guidValue}\"}}"), 9);

            Assert.Equal(9, result.Element.Timestamp.Value);
            Assert.Equal(value.StringValue, result.Element.Value.StringValue);
            Assert.Equal(value.IntValue, result.Element.Value.IntValue);
            Assert.Equal(value.DecimalValue, result.Element.Value.DecimalValue);
            Assert.Equal(longValue, result.Element.Value.NullableLongValue);
            Assert.Equal(guidValue, result.Element.Value.GuidValue);
        }

        [Theory]
        [AutoData]
        public void Update_NullableValues_SetsNulls(TestType value)
        {
            var lww = new LWW_Register<TestType>(new LWW_RegisterElement<TestType>(value, 0));

            var result = lww.Assign(JToken.Parse("{\"StringValue\": null}"), 1);
            result = result.Assign(JToken.Parse("{\"NullableLongValue\": null}"), 2);
            result = result.Assign(JToken.Parse("{\"ObjectValue\": null}"), 3);

            Assert.Null(result.Element.Value.StringValue);
            Assert.Null(result.Element.Value.NullableLongValue);
            Assert.Null(result.Element.Value.ObjectValue);
        }

        [Theory]
        [AutoData]
        public void Update_InnerObjectValues_SetsNewValues(TestType value,
            string stringValue, int intValue, decimal decimalValue)
        {
            var lww = new LWW_Register<TestType>(new LWW_RegisterElement<TestType>(value, 0));

            var result = lww.Assign(JToken.Parse($"{{\"ObjectValue\": {{ \"StringValue\": \"{stringValue}\", " +
                                                              $"\"DecimalValue\": {decimalValue}, \"IntValue\": {intValue}," +
                                                              $"\"NullableLongValue\": null }}}}"), 1);

            Assert.Equal(stringValue, result.Element.Value.ObjectValue.StringValue);
            Assert.Equal(decimalValue, result.Element.Value.ObjectValue.DecimalValue);
            Assert.Equal(intValue, result.Element.Value.ObjectValue.IntValue);
            Assert.Null(result.Element.Value.ObjectValue.NullableLongValue);
        }

        [Theory]
        [AutoData]
        public void Update_NonExistingValues_DoNotTakeEffectForValues(TestType value,
            string stringValue, int intValue, decimal decimalValue, long longValue, Guid guidValue)
        {
            var lww = new LWW_Register<TestType>(new LWW_RegisterElement<TestType>(value, 0));

            var result = lww.Assign(JToken.Parse($"{{\"FooStringValue\": \"{stringValue}\"}}"), 1);
            result = result.Assign(JToken.Parse($"{{\"FooIntValue\": {intValue}}}"), 2);
            result = result.Assign(JToken.Parse($"{{\"FooDecimalValue\": {decimalValue}}}"), 3);
            result = result.Assign(JToken.Parse($"{{\"FooNullableLongValue\": {longValue}}}"), 4);
            result = result.Assign(JToken.Parse($"{{\"FooGuidValue\": \"{guidValue}\"}}"), 5);

            Assert.Equal(result, lww);
            Assert.Equal(value, result.Element.Value);
        }

        [Theory]
        [AutoData]
        public void Update_ArrayValues_SetsNewValues(TestType value)
        {
            var lww = new LWW_Register<TestType>(new LWW_RegisterElement<TestType>(value, 0));

            var result = lww.Assign(JToken.Parse("{\"IntArray\": [1, 2, 3, 4, 5]}"), 1);
            result = result.Assign(JToken.Parse("{\"LongList\": []}"), 2);

            Assert.Equal(5, result.Element.Value.IntArray.Length);
            Assert.Empty(result.Element.Value.LongList);
        }

        [Theory]
        [AutoData]
        public void Update_ListValues_SetsNewValues(TestType value)
        {
            var lww = new LWW_Register<TestType>(new LWW_RegisterElement<TestType>(value, 0));

            var result = lww.Assign(JToken.Parse("{\"IntArray\": []}"), 1);
            result = result.Assign(JToken.Parse("{\"LongList\": [-1000, 100, 200, 300, 400, 500]}"), 2);

            Assert.Equal(6, result.Element.Value.LongList.Count);
            Assert.Empty(result.Element.Value.IntArray);
        }
    }
}