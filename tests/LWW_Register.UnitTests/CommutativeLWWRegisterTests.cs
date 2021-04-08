using System;
using Abstractions.Entities;
using AutoFixture.Xunit2;
using Cluster.Entities;
using LWW_Register.Commutative;
using Newtonsoft.Json.Linq;
using UnitTestHelpers.TestTypes;
using Xunit;
using static UnitTestHelpers.GuidHelpers;

namespace LWW_Register.UnitTests
{
    public class CommutativeLWWRegisterTests
    {
        [Theory]
        [AutoData]
        public void Merge_PrimitiveValues_SetsNewValues(TestType value, Node node, Node otherNode,
            string stringValue, int intValue, decimal decimalValue, long longValue, Guid guidValue)
        {
            var lww = new LWW_Register<TestType>(value, node, 0);

            var result = lww.Merge(new Operation(1, node, JToken.Parse($"{{\"StringValue\": \"{stringValue}\"}}")));
            result = result.Merge(new Operation(2, node, JToken.Parse($"{{\"IntValue\": {intValue}}}")));
            result = result.Merge(new Operation(3, node, JToken.Parse($"{{\"DecimalValue\": {decimalValue}}}")));
            result = result.Merge(new Operation(4, node, JToken.Parse($"{{\"NullableLongValue\": {longValue}}}")));
            result = result.Merge(new Operation(5, otherNode, JToken.Parse($"{{\"GuidValue\": \"{guidValue}\"}}")));

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
        public void Merge_OperationsWithLowerTimestamp_DoNotTakeEffect(TestType value, Node node,
            string stringValue, int intValue, decimal decimalValue, long longValue, Guid guidValue)
        {
            var lww = new LWW_Register<TestType>(value, node);

            var result = lww.Merge(new Operation(1, node, JToken.Parse($"{{\"StringValue\": \"{stringValue}\"}}")));
            result = result.Merge(new Operation(2, node, JToken.Parse($"{{\"IntValue\": {intValue}}}")));
            result = result.Merge(new Operation(3, node, JToken.Parse($"{{\"DecimalValue\": {decimalValue}}}")));
            result = result.Merge(new Operation(4, node, JToken.Parse($"{{\"NullableLongValue\": {longValue}}}")));
            result = result.Merge(new Operation(5, node, JToken.Parse($"{{\"GuidValue\": \"{guidValue}\"}}")));

            Assert.Same(result, lww);
            Assert.Same(value, result.Value);
        }

        [Theory]
        [AutoData]
        public void Merge_PrimitiveValuesWithMixedTimestamps_SetsNewValues(TestType value, Node node, Node otherNode,
            string stringValue, int intValue, decimal decimalValue, long longValue, Guid guidValue)
        {
            var lww = new LWW_Register<TestType>(value, node, 5);

            var result = lww.Merge(new Operation(1, node, JToken.Parse($"{{\"StringValue\": \"{stringValue}\"}}")));
            result = result.Merge(new Operation(2, otherNode, JToken.Parse($"{{\"IntValue\": {intValue}}}")));
            result = result.Merge(new Operation(3, otherNode, JToken.Parse($"{{\"DecimalValue\": {decimalValue}}}")));
            result = result.Merge(new Operation(8, node, JToken.Parse($"{{\"NullableLongValue\": {longValue}}}")));
            result = result.Merge(new Operation(9, node, JToken.Parse($"{{\"GuidValue\": \"{guidValue}\"}}")));

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
        public void Merge_NullableValues_SetsNulls(TestType value, Node node)
        {
            var lww = new LWW_Register<TestType>(value, node, 0);

            var result = lww.Merge(new Operation(1, node, JToken.Parse("{\"StringValue\": null}")));
            result = result.Merge(new Operation(2, node, JToken.Parse("{\"NullableLongValue\": null}")));
            result = result.Merge(new Operation(3, node, JToken.Parse("{\"ObjectValue\": null}")));

            Assert.Null(result.Value.StringValue);
            Assert.Null(result.Value.NullableLongValue);
            Assert.Null(result.Value.ObjectValue);
        }

        [Theory]
        [AutoData]
        public void Merge_InnerObjectValues_SetsNewValues(TestType value, Node node,
            string stringValue, int intValue, decimal decimalValue)
        {
            var lww = new LWW_Register<TestType>(value, node, 0);

            var result = lww.Merge(new Operation(1, node, JToken.Parse($"{{\"ObjectValue\": {{ \"StringValue\": \"{stringValue}\", " +
                                                                       $"\"DecimalValue\": {decimalValue}, \"IntValue\": {intValue}," +
                                                                       $"\"NullableLongValue\": null }}}}")));

            Assert.Equal(stringValue, result.Value.ObjectValue.StringValue);
            Assert.Equal(decimalValue, result.Value.ObjectValue.DecimalValue);
            Assert.Equal(intValue, result.Value.ObjectValue.IntValue);
            Assert.Null(result.Value.ObjectValue.NullableLongValue);
        }

        [Theory]
        [AutoData]
        public void Merge_NonExistingValues_DoNotTakeEffect(TestType value, Node node,
            string stringValue, int intValue, decimal decimalValue, long longValue, Guid guidValue)
        {
            var lww = new LWW_Register<TestType>(value, node);

            var result = lww.Merge(new Operation(1, node, JToken.Parse($"{{\"FooStringValue\": \"{stringValue}\"}}")));
            result = result.Merge(new Operation(2, node, JToken.Parse($"{{\"FooIntValue\": {intValue}}}")));
            result = result.Merge(new Operation(3, node, JToken.Parse($"{{\"FooDecimalValue\": {decimalValue}}}")));
            result = result.Merge(new Operation(4, node, JToken.Parse($"{{\"FooNullableLongValue\": {longValue}}}")));
            result = result.Merge(new Operation(5, node, JToken.Parse($"{{\"FooGuidValue\": \"{guidValue}\"}}")));

            Assert.Same(result, lww);
            Assert.Same(value, result.Value);
        }

        [Theory]
        [AutoData]
        public void Merge_NodeWithSmallerId_DoesNotTakeEffect(TestType value, string stringValue)
        {
            var node = new Node(GenerateGuid('a', Guid.Empty));
            var otherNode = new Node(GenerateGuid('b', Guid.Empty));

            var lww = new LWW_Register<TestType>(value, node, 1);

            var result = lww.Merge(new Operation(1, otherNode, JToken.Parse($"{{\"StringValue\": \"{stringValue}\"}}")));

            Assert.Same(result, lww);
            Assert.Same(value, result.Value);
        }

        [Theory]
        [AutoData]
        public void Merge_OtherNodeWithSmallerId_SetsNewValue(TestType value, string stringValue)
        {
            var node = new Node(GenerateGuid('b', Guid.Empty));
            var otherNode = new Node(GenerateGuid('a', Guid.Empty));

            var lww = new LWW_Register<TestType>(value, node, 1);

            var result = lww.Merge(new Operation(1, otherNode, JToken.Parse($"{{\"StringValue\": \"{stringValue}\"}}")));

            Assert.Equal(otherNode, result.UpdatedBy);
            Assert.Equal(stringValue, result.Value.StringValue);
        }
    }
}