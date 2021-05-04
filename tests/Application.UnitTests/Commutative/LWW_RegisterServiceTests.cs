using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Commutative.Register;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Core.Cluster;
using CRDT.Registers.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CRDT.Application.UnitTests.Commutative
{
    public class LWW_RegisterServiceTests
    {
        private readonly LWW_RegisterService<TestType> _service;
        private readonly ILWW_RegisterRepository<TestType> _repository;

        public LWW_RegisterServiceTests()
        {
            _repository = new LWW_RegisterRepository();
            _service = new LWW_RegisterService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void Update_EmptyRepository_AddsToRepository(TestType value, long timestamp)
        {
            _service.DownstreamAssign(value.Id, JToken.Parse(JsonConvert.SerializeObject(value)), timestamp);

            AssertExistsInRepository(value, timestamp);
        }

        [Theory]
        [AutoData]
        public void Update_SameValueExistsWithLowerTimestamp_DoesNotDoAnything(TestType value, long timestamp)
        {
            _repository.PersistElement(new LWW_RegisterElement<TestType>(value, timestamp - 100));

            _service.DownstreamAssign(value.Id, JToken.Parse(JsonConvert.SerializeObject(value)), timestamp);

            AssertExistsInRepository(value, timestamp - 100);
        }

        [Theory]
        [AutoData]
        public void Update_SameValueExistsWithHigherTimestamp_DoesNotDoAnything(TestType value, long timestamp)
        {
            _repository.PersistElement(new LWW_RegisterElement<TestType>(value, timestamp));

            _service.DownstreamAssign(value.Id, JToken.Parse(JsonConvert.SerializeObject(value)), timestamp - 100);

            AssertExistsInRepository(value, timestamp);
        }

        [Theory]
        [AutoData]
        public void Update_DifferentValueExistsWithLowerTimestamp_ReplacesEntity(Guid id, long timestamp)
        {
            var value = TestTypeBuilder.Build(id);
            var newValue = TestTypeBuilder.Build(id);

            _repository.PersistElement(new LWW_RegisterElement<TestType>(value, timestamp));

            _service.DownstreamAssign(value.Id, JToken.Parse(JsonConvert.SerializeObject(newValue)), timestamp + 100);

            AssertDoesNotExistInRepository(value, timestamp);
            AssertExistsInRepository(newValue, timestamp + 100);
        }

        [Theory]
        [AutoData]
        public void Update_DifferentValueExistsWithHigherTimestamp_DoesNotDoAnything(Guid id, long timestamp)
        {
            var value = TestTypeBuilder.Build(id);
            var newValue = TestTypeBuilder.Build(id);

            _repository.PersistElement(new LWW_RegisterElement<TestType>(value, timestamp + 100));

            _service.DownstreamAssign(id, JToken.Parse(JsonConvert.SerializeObject(newValue)), timestamp);

            AssertExistsInRepository(value, timestamp + 100);
            AssertDoesNotExistInRepository(newValue, timestamp);
        }

        [Theory]
        [AutoData]
        public void Update_Concurrent_AddsOnlyOneEntityWithLowerNodeValue(TestType value, long timestamp)
        {
            _service.DownstreamAssign(value.Id, JToken.Parse(JsonConvert.SerializeObject(value)), timestamp);
            _service.DownstreamAssign(value.Id, JToken.Parse(JsonConvert.SerializeObject(value)), timestamp);

            AssertExistsInRepository(value, timestamp);
        }

        [Theory]
        [AutoData]
        public void Update_PartOfState_BuildsSameValue(TestType value, long timestamp)
        {
            var operations = new List<(JToken, long)>
            {
                (JToken.Parse($"{{\"StringValue\": \"{value.StringValue}\"}}"), timestamp),
                (JToken.Parse($"{{\"IntValue\": {value.IntValue}}}"), timestamp + 1),
                (JToken.Parse($"{{\"DecimalValue\": {value.DecimalValue}}}"), timestamp + 2),
                (JToken.Parse($"{{\"NullableLongValue\": {value.NullableLongValue}}}"), timestamp + 3),
                (JToken.Parse($"{{\"GuidValue\": \"{value.GuidValue}\"}}"), timestamp + 4),
                (JToken.Parse($"{{\"IntArray\": {JsonConvert.SerializeObject(value.IntArray)}}}"), timestamp + 5),
                (JToken.Parse($"{{\"LongList\": {JsonConvert.SerializeObject(value.LongList)}}}"), timestamp + 6),
                (JToken.Parse($"{{\"ObjectValue\": {JsonConvert.SerializeObject(value.ObjectValue)}}}"), timestamp + 7),
            };

            foreach (var operation in operations)
            {
                _service.DownstreamAssign(value.Id, operation.Item1, operation.Item2);
            }

            AssertExistsInRepository(value, timestamp + 7);
        }

        [Theory]
        [AutoData]
        public void Update_MixedTimestamps_BuildsCorrectValue(TestType value, long timestamp)
        {
            var operations = new List<(JToken, long)>
            {
                (JToken.Parse($"{{\"StringValue\": \"{value.StringValue}\"}}"), timestamp),
                (JToken.Parse($"{{\"IntValue\": 999}}"), timestamp + 1),
                (JToken.Parse($"{{\"IntArray\": null}}"), timestamp + 3),
                (JToken.Parse($"{{\"DecimalValue\": {value.DecimalValue}}}"), timestamp + 2),
                (JToken.Parse($"{{\"NullableLongValue\": {value.NullableLongValue}}}"), timestamp + 3),
                (JToken.Parse($"{{\"NotExistingValue\": {value.NullableLongValue}}}"), timestamp + 3),
                (JToken.Parse($"{{\"GuidValue\": \"{value.GuidValue}\"}}"), timestamp + 4),
                (JToken.Parse($"{{\"LongList\": {JsonConvert.SerializeObject(value.LongList)}}}"), timestamp + 6),
                (JToken.Parse($"{{\"ObjectValue\": {JsonConvert.SerializeObject(value.ObjectValue)}}}"), timestamp + 7),
                (JToken.Parse($"{{\"DecimalValue\": 1000.5}}"), timestamp + 7),
            };

            foreach (var operation in operations)
            {
                _service.DownstreamAssign(value.Id, operation.Item1, operation.Item2);
            }

            var expected = new TestType(value.Id)
            {
                StringValue = value.StringValue,
                IntValue = 999,
                DecimalValue = value.DecimalValue,
                ObjectValue = value.ObjectValue,
                LongList = value.LongList,
                IntArray = null,
                NullableLongValue = value.NullableLongValue,
                GuidValue = value.GuidValue
            };

            AssertExistsInRepository(expected, timestamp + 7);
        }

        [Fact]
        public void Value_NotExistingEntity_ReturnsNull()
        {
            var value = _service.GetValue(Guid.NewGuid());

            Assert.Null(value);
        }

        [Theory]
        [AutoData]
        public void Value_ExistingEntity_ReturnsValue(TestType value, long timestamp)
        {
            _service.DownstreamAssign(value.Id, JToken.Parse(JsonConvert.SerializeObject(value)), timestamp);

            var actualValue = _service.GetValue(value.Id);

            Assert.Equal(value, actualValue);
        }

        [Fact]
        public void Commutative_Assign_UpdateSingleField()
        {
            var nodes = CreateNodes(3);
            var commutativeReplicas = CreateCommutativeReplicas(nodes);

            var initialValue = TestTypeBuilder.Build();
            var valueId = initialValue.Id;

            long ts = 0;

            var firstReplica = commutativeReplicas.First();
            firstReplica.Value.LocalAssign(valueId, JToken.FromObject(initialValue), ts);

            CommutativeDownstreamAssign(firstReplica.Key.Id, valueId, JToken.FromObject(firstReplica.Value.GetValue(valueId)), ts, commutativeReplicas);

            ts++;

            foreach (var replica in commutativeReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    initialValue.StringValue = Guid.NewGuid().ToString();

                    var jToken = JToken.Parse($"{{\"StringValue\":\"{initialValue.StringValue}\"}}");

                    replica.Value.LocalAssign(valueId, jToken, ts);

                    CommutativeDownstreamAssign(replica.Key.Id, valueId, jToken, ts, commutativeReplicas);

                    ts++;
                }
            }

            foreach (var replica in commutativeReplicas)
            {
                Assert.Equal(initialValue, replica.Value.GetValue(valueId));
            }
        }

        private List<Node> CreateNodes(int count)
        {
            var nodes = new List<Node>();

            for (var i = 0; i < count; i++)
            {
                nodes.Add(new Node());
            }

            return nodes;
        }

        private Dictionary<Node, LWW_RegisterService<TestType>> CreateCommutativeReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, LWW_RegisterService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new LWW_RegisterRepository();
                var service = new LWW_RegisterService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void CommutativeDownstreamAssign(Guid senderId, Guid objectId, JToken value, long timestamp, Dictionary<Node, LWW_RegisterService<TestType>> replicas)
        {
            var downstreamReplicas = replicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.DownstreamAssign(objectId, value, timestamp);
            }
        }

        private void AssertExistsInRepository(TestType value, long timestamp)
        {
            Assert.Equal(1, _repository.GetElements().Count(e => Equals(e.Value, value) &&
                                                            e.Timestamp.Value == timestamp));
        }

        private void AssertDoesNotExistInRepository(TestType value, long timestamp)
        {
            Assert.DoesNotContain(_repository.GetElements(),
                e => Equals(e.Value, value) &&
                     e.Timestamp.Value == timestamp);
        }
    }
}