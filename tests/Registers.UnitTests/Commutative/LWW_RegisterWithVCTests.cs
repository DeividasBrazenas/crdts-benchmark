using System;
using System.Collections.Immutable;
using AutoFixture.Xunit2;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using CRDT.Registers.Commutative.LastWriterWins;
using CRDT.Registers.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CRDT.Registers.UnitTests.Commutative
{
    public class LWW_RegisterWithVCTests
    {
        [Theory]
        [AutoData]
        public void Update_PrimitiveValues_SetsNewValues(TestType value, Node node,
            string stringValue, int intValue, decimal decimalValue, long longValue, Guid guidValue)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lww = new LWW_RegisterWithVC<TestType>(new LWW_RegisterWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false));

            var result = lww.Assign(JToken.Parse($"{{\"StringValue\": \"{stringValue}\"}}"), new VectorClock(clock.Add(node, 1)));
            result = result.Assign(JToken.Parse($"{{\"IntValue\": {intValue}}}"), new VectorClock(clock.Add(node, 2)));
            result = result.Assign(JToken.Parse($"{{\"DecimalValue\": {decimalValue}}}"), new VectorClock(clock.Add(node, 3)));
            result = result.Assign(JToken.Parse($"{{\"NullableLongValue\": {longValue}}}"), new VectorClock(clock.Add(node, 4)));
            result = result.Assign(JToken.Parse($"{{\"GuidValue\": \"{guidValue}\"}}"), new VectorClock(clock.Add(node, 5)));

            Assert.Equal(new VectorClock(clock.Add(node, 5)), result.Element.VectorClock);
            Assert.Equal(stringValue, result.Element.Value.StringValue);
            Assert.Equal(intValue, result.Element.Value.IntValue);
            Assert.Equal(decimalValue, result.Element.Value.DecimalValue);
            Assert.Equal(longValue, result.Element.Value.NullableLongValue);
            Assert.Equal(guidValue, result.Element.Value.GuidValue);
        }

        [Theory]
        [AutoData]
        public void Update_OperationsWithLowerTimestamp_DoNotTakeEffect(TestType value, Node node,
            string stringValue, int intValue, decimal decimalValue, long longValue, Guid guidValue)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lww = new LWW_RegisterWithVC<TestType>(new LWW_RegisterWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 10)), false));

            var result = lww.Assign(JToken.Parse($"{{\"StringValue\": \"{stringValue}\"}}"), new VectorClock(clock.Add(node, 1)));
            result = result.Assign(JToken.Parse($"{{\"IntValue\": {intValue}}}"), new VectorClock(clock.Add(node, 2)));
            result = result.Assign(JToken.Parse($"{{\"DecimalValue\": {decimalValue}}}"), new VectorClock(clock.Add(node, 3)));
            result = result.Assign(JToken.Parse($"{{\"NullableLongValue\": {longValue}}}"), new VectorClock(clock.Add(node, 4)));
            result = result.Assign(JToken.Parse($"{{\"GuidValue\": \"{guidValue}\"}}"), new VectorClock(clock.Add(node, 5)));

            Assert.Equal(result, lww);
            Assert.Equal(value, result.Element.Value);
        }

        [Theory]
        [AutoData]
        public void Update_PrimitiveValuesWithMixedTimestamps_SetsNewValues(TestType value, Node node,
            string stringValue, int intValue, decimal decimalValue, long longValue, Guid guidValue)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lww = new LWW_RegisterWithVC<TestType>(new LWW_RegisterWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 5)), false));

            var result = lww.Assign(JToken.Parse($"{{\"StringValue\": \"{stringValue}\"}}"), new VectorClock(clock.Add(node, 1)));
            result = result.Assign(JToken.Parse($"{{\"IntValue\": {intValue}}}"), new VectorClock(clock.Add(node, 2)));
            result = result.Assign(JToken.Parse($"{{\"DecimalValue\": {decimalValue}}}"), new VectorClock(clock.Add(node, 3)));
            result = result.Assign(JToken.Parse($"{{\"NullableLongValue\": {longValue}}}"), new VectorClock(clock.Add(node, 8)));
            result = result.Assign(JToken.Parse($"{{\"GuidValue\": \"{guidValue}\"}}"), new VectorClock(clock.Add(node, 9)));

            Assert.Equal(new VectorClock(clock.Add(node, 9)), result.Element.VectorClock);
            Assert.Equal(value.StringValue, result.Element.Value.StringValue);
            Assert.Equal(value.IntValue, result.Element.Value.IntValue);
            Assert.Equal(value.DecimalValue, result.Element.Value.DecimalValue);
            Assert.Equal(longValue, result.Element.Value.NullableLongValue);
            Assert.Equal(guidValue, result.Element.Value.GuidValue);
        }

        [Theory]
        [AutoData]
        public void Update_NullableValues_SetsNulls(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lww = new LWW_RegisterWithVC<TestType>(new LWW_RegisterWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false));

            var result = lww.Assign(JToken.Parse("{\"StringValue\": null}"), new VectorClock(clock.Add(node, 1)));
            result = result.Assign(JToken.Parse("{\"NullableLongValue\": null}"), new VectorClock(clock.Add(node, 2)));
            result = result.Assign(JToken.Parse("{\"ObjectValue\": null}"), new VectorClock(clock.Add(node, 3)));

            Assert.Null(result.Element.Value.StringValue);
            Assert.Null(result.Element.Value.NullableLongValue);
            Assert.Null(result.Element.Value.ObjectValue);
        }

        [Theory]
        [AutoData]
        public void Update_InnerObjectValues_SetsNewValues(TestType value, Node node,
            string stringValue, int intValue, decimal decimalValue)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lww = new LWW_RegisterWithVC<TestType>(new LWW_RegisterWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false));

            var result = lww.Assign(JToken.Parse($"{{\"ObjectValue\": {{ \"StringValue\": \"{stringValue}\", " +
                                                              $"\"DecimalValue\": {decimalValue}, \"IntValue\": {intValue}," +
                                                              $"\"NullableLongValue\": null }}}}"), new VectorClock(clock.Add(node, 1)));

            Assert.Equal(stringValue, result.Element.Value.ObjectValue.StringValue);
            Assert.Equal(decimalValue, result.Element.Value.ObjectValue.DecimalValue);
            Assert.Equal(intValue, result.Element.Value.ObjectValue.IntValue);
            Assert.Null(result.Element.Value.ObjectValue.NullableLongValue);
        }

        [Theory]
        [AutoData]
        public void Update_NonExistingValues_DoNotTakeEffectForValues(TestType value, Node node,
            string stringValue, int intValue, decimal decimalValue, long longValue, Guid guidValue)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lww = new LWW_RegisterWithVC<TestType>(new LWW_RegisterWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false));

            var result = lww.Assign(JToken.Parse($"{{\"FooStringValue\": \"{stringValue}\"}}"), new VectorClock(clock.Add(node, 1)));
            result = result.Assign(JToken.Parse($"{{\"FooIntValue\": {intValue}}}"), new VectorClock(clock.Add(node, 2)));
            result = result.Assign(JToken.Parse($"{{\"FooDecimalValue\": {decimalValue}}}"), new VectorClock(clock.Add(node, 3)));
            result = result.Assign(JToken.Parse($"{{\"FooNullableLongValue\": {longValue}}}"), new VectorClock(clock.Add(node, 4)));
            result = result.Assign(JToken.Parse($"{{\"FooGuidValue\": \"{guidValue}\"}}"), new VectorClock(clock.Add(node, 5)));

            Assert.Equal(result, lww);
            Assert.Equal(value, result.Element.Value);
        }

        [Theory]
        [AutoData]
        public void Update_ArrayValues_SetsNewValues(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lww = new LWW_RegisterWithVC<TestType>(new LWW_RegisterWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false));

            var result = lww.Assign(JToken.Parse("{\"IntArray\": [1, 2, 3, 4, 5]}"), new VectorClock(clock.Add(node, 1)));
            result = result.Assign(JToken.Parse("{\"LongList\": []}"), new VectorClock(clock.Add(node, 2)));

            Assert.Equal(5, result.Element.Value.IntArray.Length);
            Assert.Empty(result.Element.Value.LongList);
        }

        [Theory]
        [AutoData]
        public void Update_ListValues_SetsNewValues(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lww = new LWW_RegisterWithVC<TestType>(new LWW_RegisterWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false));

            var result = lww.Assign(JToken.Parse("{\"IntArray\": []}"), new VectorClock(clock.Add(node, 1)));
            result = result.Assign(JToken.Parse("{\"LongList\": [-1000, 100, 200, 300, 400, 500]}"), new VectorClock(clock.Add(node, 2)));

            Assert.Equal(6, result.Element.Value.LongList.Count);
            Assert.Empty(result.Element.Value.IntArray);
        }
    }
}