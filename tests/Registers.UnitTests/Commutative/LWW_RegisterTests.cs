using System;
using AutoFixture.Xunit2;
using CRDT.Core.Cluster;
using CRDT.Registers.Commutative;
using CRDT.Registers.Operations;
using CRDT.UnitTestHelpers.TestTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using static CRDT.UnitTestHelpers.GuidHelpers;

namespace CRDT.Registers.UnitTests.Commutative
{
    public class LWW_RegisterTests
    {
        [Theory]
        [AutoData]
        public void Update_PrimitiveValues_SetsNewValues(TestType value, Node node, Node otherNode,
            string stringValue, int intValue, decimal decimalValue, long longValue, Guid guidValue)
        {
            var lww = new LWW_Register<TestType>(value, node, 0);

            var result = lww.Update(new Operation(JToken.Parse($"{{\"StringValue\": \"{stringValue}\"}}"), 1, node));
            result = result.Update(new Operation(JToken.Parse($"{{\"IntValue\": {intValue}}}"), 2, node));
            result = result.Update(new Operation(JToken.Parse($"{{\"DecimalValue\": {decimalValue}}}"), 3, node));
            result = result.Update(new Operation(JToken.Parse($"{{\"NullableLongValue\": {longValue}}}"), 4, node));
            result = result.Update(new Operation(JToken.Parse($"{{\"GuidValue\": \"{guidValue}\"}}"), 5, otherNode));

            Assert.Same(otherNode, result.UpdatedBy);
            Assert.Equal(5, result.Timestamp.Value);
            Assert.Equal(stringValue, result.Value.StringValue);
            Assert.Equal(intValue, result.Value.IntValue);
            Assert.Equal(decimalValue, result.Value.DecimalValue);
            Assert.Equal(longValue, result.Value.NullableLongValue);
            Assert.Equal(guidValue, result.Value.GuidValue);
        }
        [Theory]
        [AutoData]
        public void Update_OperationsWithLowerTimestamp_DoNotTakeEffect(TestType value, Node node,
            string stringValue, int intValue, decimal decimalValue, long longValue, Guid guidValue)
        {
            var lww = new LWW_Register<TestType>(value, node, 10);

            var result = lww.Update(new Operation(JToken.Parse($"{{\"StringValue\": \"{stringValue}\"}}"), 1, node));
            result = result.Update(new Operation(JToken.Parse($"{{\"IntValue\": {intValue}}}"), 2, node));
            result = result.Update(new Operation(JToken.Parse($"{{\"DecimalValue\": {decimalValue}}}"), 3, node));
            result = result.Update(new Operation(JToken.Parse($"{{\"NullableLongValue\": {longValue}}}"), 4, node));
            result = result.Update(new Operation(JToken.Parse($"{{\"GuidValue\": \"{guidValue}\"}}"), 5, node));

            Assert.Equal(result, lww);
            Assert.Equal(value, result.Value);
        }
        [Theory]
        [AutoData]
        public void Update_PrimitiveValuesWithMixedTimestamps_SetsNewValues(TestType value, Node node, Node otherNode,
            string stringValue, int intValue, decimal decimalValue, long longValue, Guid guidValue)
        {
            var lww = new LWW_Register<TestType>(value, node, 5);

            var result = lww.Update(new Operation(JToken.Parse($"{{\"StringValue\": \"{stringValue}\"}}"), 1, node));
            result = result.Update(new Operation(JToken.Parse($"{{\"IntValue\": {intValue}}}"), 2, otherNode));
            result = result.Update(new Operation(JToken.Parse($"{{\"DecimalValue\": {decimalValue}}}"), 3, otherNode));
            result = result.Update(new Operation(JToken.Parse($"{{\"NullableLongValue\": {longValue}}}"), 8, node));
            result = result.Update(new Operation(JToken.Parse($"{{\"GuidValue\": \"{guidValue}\"}}"), 9, node));

            Assert.Same(node, result.UpdatedBy);
            Assert.Equal(9, result.Timestamp.Value);
            Assert.Equal(value.StringValue, result.Value.StringValue);
            Assert.Equal(value.IntValue, result.Value.IntValue);
            Assert.Equal(value.DecimalValue, result.Value.DecimalValue);
            Assert.Equal(longValue, result.Value.NullableLongValue);
            Assert.Equal(guidValue, result.Value.GuidValue);
        }
        [Theory]
        [AutoData]
        public void Update_NullableValues_SetsNulls(TestType value, Node node)
        {
            var lww = new LWW_Register<TestType>(value, node, 0);

            var result = lww.Update(new Operation(JToken.Parse("{\"StringValue\": null}"), 1, node));
            result = result.Update(new Operation(JToken.Parse("{\"NullableLongValue\": null}"), 2, node));
            result = result.Update(new Operation(JToken.Parse("{\"ObjectValue\": null}"), 3, node));

            Assert.Null(result.Value.StringValue);
            Assert.Null(result.Value.NullableLongValue);
            Assert.Null(result.Value.ObjectValue);
        }
        [Theory]
        [AutoData]
        public void Update_InnerObjectValues_SetsNewValues(TestType value, Node node,
            string stringValue, int intValue, decimal decimalValue)
        {
            var lww = new LWW_Register<TestType>(value, node, 0);

            var result = lww.Update(new Operation(JToken.Parse($"{{\"ObjectValue\": {{ \"StringValue\": \"{stringValue}\", " +
                                                              $"\"DecimalValue\": {decimalValue}, \"IntValue\": {intValue}," +
                                                              $"\"NullableLongValue\": null }}}}"), 1, node));

            Assert.Equal(stringValue, result.Value.ObjectValue.StringValue);
            Assert.Equal(decimalValue, result.Value.ObjectValue.DecimalValue);
            Assert.Equal(intValue, result.Value.ObjectValue.IntValue);
            Assert.Null(result.Value.ObjectValue.NullableLongValue);
        }
        [Theory]
        [AutoData]
        public void Update_NonExistingValues_DoNotTakeEffect(TestType value, Node node,
            string stringValue, int intValue, decimal decimalValue, long longValue, Guid guidValue)
        {
            var lww = new LWW_Register<TestType>(value, node, 0);

            var result = lww.Update(new Operation(JToken.Parse($"{{\"FooStringValue\": \"{stringValue}\"}}"), 1, node));
            result = result.Update(new Operation(JToken.Parse($"{{\"FooIntValue\": {intValue}}}"), 2, node));
            result = result.Update(new Operation(JToken.Parse($"{{\"FooDecimalValue\": {decimalValue}}}"), 3, node));
            result = result.Update(new Operation(JToken.Parse($"{{\"FooNullableLongValue\": {longValue}}}"), 4, node));
            result = result.Update(new Operation(JToken.Parse($"{{\"FooGuidValue\": \"{guidValue}\"}}"), 5, node));

            Assert.Equal(result, lww);
            Assert.Equal(value, result.Value);
        }
        [Theory]
        [AutoData]
        public void Update_ArrayValues_SetsNewValues(TestType value, Node node)
        {
            var lww = new LWW_Register<TestType>(value, node, 0);

            var result = lww.Update(new Operation(JToken.Parse("{\"IntArray\": [1, 2, 3, 4, 5]}"), 1, node));
            result = result.Update(new Operation(JToken.Parse("{\"LongList\": []}"), 2, node));

            Assert.Equal(5, result.Value.IntArray.Length);
            Assert.Empty(result.Value.LongList);
        }
        [Theory]
        [AutoData]
        public void Update_ListValues_SetsNewValues(TestType value, Node node)
        {
            var lww = new LWW_Register<TestType>(value, node, 0);

            var result = lww.Update(new Operation(JToken.Parse("{\"IntArray\": []}"), 1, node));
            result = result.Update(new Operation(JToken.Parse("{\"LongList\": [-1000, 100, 200, 300, 400, 500]}"), 2, node));

            Assert.Equal(6, result.Value.LongList.Count);
            Assert.Empty(result.Value.IntArray);
        }
        [Theory]
        [AutoData]
        public void Update_NodeWithSmallerId_DoesNotTakeEffect(TestType value, string stringValue)
        {
            var node = new Node(GenerateGuid('a', Guid.Empty));
            var otherNode = new Node(GenerateGuid('b', Guid.Empty));

            var lww = new LWW_Register<TestType>(value, node, 1);

            var result = lww.Update(new Operation(JToken.Parse($"{{\"StringValue\": \"{stringValue}\"}}"), 1, otherNode));

            Assert.Same(result, lww);
            Assert.Same(value, result.Value);
        }
        [Theory]
        [AutoData]
        public void Update_OtherNodeWithSmallerId_SetsNewValue(TestType value, string stringValue)
        {
            var node = new Node(GenerateGuid('b', Guid.Empty));
            var otherNode = new Node(GenerateGuid('a', Guid.Empty));

            var lww = new LWW_Register<TestType>(value, node, 1);

            var result = lww.Update(new Operation(JToken.Parse($"{{\"StringValue\": \"{stringValue}\"}}"), 1, otherNode));

            Assert.Equal(otherNode, result.UpdatedBy);
            Assert.Equal(stringValue, result.Value.StringValue);
        }
    }
}