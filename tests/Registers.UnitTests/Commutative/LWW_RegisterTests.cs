using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public void Create_DifferentElementIds_ThrowsException(TestType value, Node node, long timestamp)
        {
            var operations = new List<Operation>
            {
                new(value.Id, JToken.Parse(JsonConvert.SerializeObject(value)),timestamp, node),
                new(Guid.NewGuid(), JToken.Parse($"{{\"StringValue\": \"{value.GuidValue}\"}}"),timestamp + 100, node),
            };

            Assert.Throws<ArgumentException>(() => new LWW_Register<TestType>(operations.ToImmutableHashSet()));
        }

        [Theory]
        [AutoData]
        public void Value_ConstructIdenticalObject(TestType value, Node node, long timestamp)
        {
            var operations = new List<Operation>
            {
                new(value.Id, JToken.Parse($"{{\"StringValue\": \"{value.StringValue}\"}}"),timestamp, node),
                new(value.Id, JToken.Parse($"{{\"IntValue\": {value.IntValue}}}"),timestamp + 1, node),
                new(value.Id, JToken.Parse($"{{\"DecimalValue\": {value.DecimalValue}}}"),timestamp + 2, node),
                new(value.Id, JToken.Parse($"{{\"NullableLongValue\": {value.NullableLongValue}}}"),timestamp + 3, node),
                new(value.Id, JToken.Parse($"{{\"GuidValue\": \"{value.GuidValue}\"}}"),timestamp + 4, node),
                new(value.Id, JToken.Parse($"{{\"IntArray\": {JsonConvert.SerializeObject(value.IntArray)}}}"),timestamp + 5, node),
                new(value.Id, JToken.Parse($"{{\"LongList\": {JsonConvert.SerializeObject(value.LongList)}}}"),timestamp + 6, node),
                new(value.Id, JToken.Parse($"{{\"ObjectValue\": {JsonConvert.SerializeObject(value.ObjectValue)}}}"),timestamp + 7, node),
            };

            var lww = new LWW_Register<TestType>(operations.ToImmutableHashSet());

            var actualValue = lww.Value();

            Assert.Equal(value, actualValue);
        }

        [Theory]
        [AutoData]
        public void Value_OperationsWithLowerTimestamp_DoNotTakeEffect(TestType value, Node node, long timestamp)
        {
            var operations = new List<Operation>
            {
                new(value.Id, JToken.Parse($"{{\"StringValue\": \"{value.StringValue}\"}}"),timestamp, node),
                new(value.Id, JToken.Parse($"{{\"StringValue\": null}}"),timestamp - 100, node),
            };

            var lww = new LWW_Register<TestType>(operations.ToImmutableHashSet());

            var actualValue = lww.Value();

            Assert.Equal(value.StringValue, actualValue.StringValue);
        }

        [Theory]
        [AutoData]
        public void Value_NonExistingValues_DoNotTakeEffect(TestType value, Node node, long timestamp)
        {
            var operations = new List<Operation>
            {
                new(value.Id, JToken.Parse(JsonConvert.SerializeObject(value)),timestamp, node),
                new(value.Id, JToken.Parse("{\"FooStringValue\": null}"),timestamp - 100, node),
                new(value.Id, JToken.Parse("{\"FooIntValue\": 999}"),timestamp - 100, node),
            };

            var lww = new LWW_Register<TestType>(operations.ToImmutableHashSet());

            var actualValue = lww.Value();

            Assert.Equal(value, actualValue);
        }

        [Theory]
        [AutoData]
        public void Value_ConcurrentTimestamps_NodeWithLowerIdWins(TestType value, long timestamp)
        {
            var firstNode = new Node(GenerateGuid('a'));
            var secondNode = new Node(GenerateGuid('b'));

            var firstStringValue = Guid.NewGuid().ToString();
            var secondStringValue = Guid.NewGuid().ToString();

            var operations = new List<Operation>
            {
                new(value.Id, JToken.Parse(JsonConvert.SerializeObject(value)),timestamp, firstNode),
                new(value.Id, JToken.Parse($"{{\"StringValue\": \"{firstStringValue}\"}}"),timestamp + 100, firstNode),
                new(value.Id, JToken.Parse($"{{\"StringValue\": \"{secondStringValue}\"}}"),timestamp + 100, secondNode),
            };

            var lww = new LWW_Register<TestType>(operations.ToImmutableHashSet());

            var actualValue = lww.Value();

            Assert.Equal(firstStringValue, actualValue.StringValue);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesOperations(Guid id, long timestamp, Node node, JToken oneJToken, JToken twoJToken, JToken threeJToken)
        {
            var one = new Operation(id, oneJToken, timestamp, node);
            var two = new Operation(id, twoJToken, timestamp + 100, node);
            var three = new Operation(id, threeJToken, timestamp + 1000, node);

            var firstLww = new LWW_Register<TestType>(new[] { one, two }.ToImmutableHashSet());
            var secondLww = new LWW_Register<TestType>(new[] { two, three }.ToImmutableHashSet());

            var lww = firstLww.Merge(secondLww);

            Assert.Equal(3, lww.Operations.Count);
            Assert.Contains(one, lww.Operations);
            Assert.Contains(two, lww.Operations);
            Assert.Contains(three, lww.Operations);
        }
    }
}