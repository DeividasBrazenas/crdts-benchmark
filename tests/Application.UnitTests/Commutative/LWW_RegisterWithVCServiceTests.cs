using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Commutative.Register;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using CRDT.Registers.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CRDT.Application.UnitTests.Commutative
{
    public class LWW_RegisterWithVCServiceTests
    {
        private readonly LWW_RegisterWithVCService<TestType> _service;
        private readonly ILWW_RegisterWithVCRepository<TestType> _repository;
        private readonly TestTypeBuilder _builder;

        public LWW_RegisterWithVCServiceTests()
        {
            _repository = new LWW_RegisterWithVCRepository();
            _service = new LWW_RegisterWithVCService<TestType>(_repository);
            _builder = new TestTypeBuilder(new Random());
        }

        [Theory]
        [AutoData]
        public void Update_EmptyRepository_AddsToRepository(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _service.DownstreamAssign(value.Id, JToken.Parse(JsonConvert.SerializeObject(value)), new VectorClock(clock.Add(node, 0)));

            AssertExistsInRepository(value, new VectorClock(clock.Add(node, 0)));
        }

        [Theory]
        [AutoData]
        public void Update_SameValueExistsWithLowerTimestamp_DoesNotDoAnything(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistElement(new LWW_RegisterWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false));

            _service.DownstreamAssign(value.Id, JToken.Parse(JsonConvert.SerializeObject(value)), new VectorClock(clock.Add(node, 1)));

            AssertExistsInRepository(value, new VectorClock(clock.Add(node, 0)));
        }

        [Theory]
        [AutoData]
        public void Update_SameValueExistsWithHigherTimestamp_DoesNotDoAnything(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistElement(new LWW_RegisterWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)), false));

            _service.DownstreamAssign(value.Id, JToken.Parse(JsonConvert.SerializeObject(value)), new VectorClock(clock.Add(node, 0)));

            AssertExistsInRepository(value, new VectorClock(clock.Add(node, 1)));
        }

        [Theory]
        [AutoData]
        public void Update_DifferentValueExistsWithLowerTimestamp_ReplacesEntity(Guid id, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var value = _builder.Build(id);
            var newValue = _builder.Build(id);

            _repository.PersistElement(new LWW_RegisterWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false));

            _service.DownstreamAssign(value.Id, JToken.Parse(JsonConvert.SerializeObject(newValue)), new VectorClock(clock.Add(node, 1)));

            AssertDoesNotExistInRepository(value, new VectorClock(clock.Add(node, 0)));
            AssertExistsInRepository(newValue, new VectorClock(clock.Add(node, 1)));
        }

        [Theory]
        [AutoData]
        public void Update_DifferentValueExistsWithHigherTimestamp_DoesNotDoAnything(Guid id, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var value = _builder.Build(id);
            var newValue = _builder.Build(id);

            _repository.PersistElement(new LWW_RegisterWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)), false));

            _service.DownstreamAssign(id, JToken.Parse(JsonConvert.SerializeObject(newValue)), new VectorClock(clock.Add(node, 0)));

            AssertExistsInRepository(value, new VectorClock(clock.Add(node, 1)));
            AssertDoesNotExistInRepository(newValue, new VectorClock(clock.Add(node, 0)));
        }

        [Theory]
        [AutoData]
        public void Update_Concurrent_AddsOnlyOneEntityWithLowerNodeValue(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _service.DownstreamAssign(value.Id, JToken.Parse(JsonConvert.SerializeObject(value)), new VectorClock(clock.Add(node, 0)));
            _service.DownstreamAssign(value.Id, JToken.Parse(JsonConvert.SerializeObject(value)), new VectorClock(clock.Add(node, 0)));

            AssertExistsInRepository(value, new VectorClock(clock.Add(node, 0)));
        }

        [Theory]
        [AutoData]
        public void Update_PartOfState_BuildsSameValue(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var operations = new List<(JToken, VectorClock)>
            {
                (JToken.Parse($"{{\"StringValue\": \"{value.StringValue}\"}}"), new VectorClock(clock.Add(node, 0))),
                (JToken.Parse($"{{\"IntValue\": {value.IntValue}}}"), new VectorClock(clock.Add(node, 1))),
                (JToken.Parse($"{{\"DecimalValue\": {value.DecimalValue}}}"), new VectorClock(clock.Add(node, 2))),
                (JToken.Parse($"{{\"NullableLongValue\": {value.NullableLongValue}}}"), new VectorClock(clock.Add(node, 3))),
                (JToken.Parse($"{{\"GuidValue\": \"{value.GuidValue}\"}}"), new VectorClock(clock.Add(node, 4))),
                (JToken.Parse($"{{\"IntArray\": {JsonConvert.SerializeObject(value.IntArray)}}}"), new VectorClock(clock.Add(node, 5))),
                (JToken.Parse($"{{\"LongList\": {JsonConvert.SerializeObject(value.LongList)}}}"), new VectorClock(clock.Add(node, 6))),
                (JToken.Parse($"{{\"ObjectValue\": {JsonConvert.SerializeObject(value.ObjectValue)}}}"), new VectorClock(clock.Add(node, 7))),
            };

            foreach (var operation in operations)
            {
                _service.DownstreamAssign(value.Id, operation.Item1, operation.Item2);
            }

            AssertExistsInRepository(value, new VectorClock(clock.Add(node, 7)));
        }

        [Theory]
        [AutoData]
        public void Update_MixedTimestamps_BuildsCorrectValue(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var operations = new List<(JToken, VectorClock)>
            {
                (JToken.Parse($"{{\"StringValue\": \"{value.StringValue}\"}}"), new VectorClock(clock.Add(node, 0))),
                (JToken.Parse($"{{\"IntValue\": 999}}"), new VectorClock(clock.Add(node, 1))),
                (JToken.Parse($"{{\"IntArray\": null}}"), new VectorClock(clock.Add(node, 3))),
                (JToken.Parse($"{{\"DecimalValue\": {value.DecimalValue}}}"), new VectorClock(clock.Add(node, 2))),
                (JToken.Parse($"{{\"NullableLongValue\": {value.NullableLongValue}}}"), new VectorClock(clock.Add(node, 3))),
                (JToken.Parse($"{{\"NotExistingValue\": {value.NullableLongValue}}}"), new VectorClock(clock.Add(node, 3))),
                (JToken.Parse($"{{\"GuidValue\": \"{value.GuidValue}\"}}"), new VectorClock(clock.Add(node, 4))),
                (JToken.Parse($"{{\"LongList\": {JsonConvert.SerializeObject(value.LongList)}}}"), new VectorClock(clock.Add(node, 6))),
                (JToken.Parse($"{{\"ObjectValue\": {JsonConvert.SerializeObject(value.ObjectValue)}}}"), new VectorClock(clock.Add(node, 7))),
                (JToken.Parse($"{{\"DecimalValue\": 1000.5}}"), new VectorClock(clock.Add(node, 7))),
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

            AssertExistsInRepository(expected, new VectorClock(clock.Add(node, 7)));
        }

        [Fact]
        public void Value_NotExistingEntity_ReturnsNull()
        {
            var value = _service.GetValue(Guid.NewGuid());

            Assert.Null(value);
        }

        [Theory]
        [AutoData]
        public void Value_ExistingEntity_ReturnsValue(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _service.DownstreamAssign(value.Id, JToken.Parse(JsonConvert.SerializeObject(value)), new VectorClock(clock.Add(node, 0)));

            var actualValue = _service.GetValue(value.Id);

            Assert.Equal(value, actualValue.Value);
        }

        [Fact]
        public void Commutative_Assign_NewValue()
        {
            var nodes = CreateNodes(3);
            var commutativeReplicas = CreateCommutativeReplicas(nodes);

            var initialValue = _builder.Build();
            var valueId = initialValue.Id;

            var clock = new VectorClock(nodes);

            var firstReplica = commutativeReplicas.First();
            firstReplica.Value.LocalAssign(valueId, JToken.FromObject(initialValue), clock);

            CommutativeDownstreamAssign(firstReplica.Key.Id, valueId, JToken.FromObject(firstReplica.Value.GetValue(valueId).Value), clock, commutativeReplicas);

            clock = clock.Increment(firstReplica.Key);

            foreach (var replica in commutativeReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    initialValue = _builder.Build(valueId);

                    replica.Value.LocalAssign(valueId, JToken.FromObject(initialValue), clock);

                    CommutativeDownstreamAssign(replica.Key.Id, valueId, JToken.FromObject(replica.Value.GetValue(valueId).Value), clock, commutativeReplicas);

                    clock = clock.Increment(replica.Key);
                }
            }

            foreach (var replica in commutativeReplicas)
            {
                Assert.Equal(initialValue, replica.Value.GetValue(valueId).Value);
            }
        }

        [Fact]
        public void Commutative_Assign_UpdateSingleField()
        {
            var nodes = CreateNodes(3);
            var commutativeReplicas = CreateCommutativeReplicas(nodes);

            var initialValue = _builder.Build();
            var valueId = initialValue.Id;

            var clock = new VectorClock(nodes);

            var firstReplica = commutativeReplicas.First();
            firstReplica.Value.LocalAssign(valueId, JToken.FromObject(initialValue), clock);

            CommutativeDownstreamAssign(firstReplica.Key.Id, valueId, JToken.FromObject(firstReplica.Value.GetValue(valueId).Value), clock, commutativeReplicas);

            clock = clock.Increment(firstReplica.Key);

            foreach (var replica in commutativeReplicas)
            {
                for (int i = 0; i < 1; i++)
                {
                    initialValue.StringValue = Guid.NewGuid().ToString();

                    var jToken = JToken.Parse($"{{\"StringValue\":\"{initialValue.StringValue}\"}}");

                    replica.Value.LocalAssign(valueId, jToken, clock);

                    CommutativeDownstreamAssign(replica.Key.Id, valueId, jToken, clock, commutativeReplicas);

                    clock = clock.Increment(replica.Key);
                }
            }

            foreach (var replica in commutativeReplicas)
            {
                Assert.Equal(initialValue, replica.Value.GetValue(valueId).Value);
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

        private Dictionary<Node, LWW_RegisterWithVCService<TestType>> CreateCommutativeReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, LWW_RegisterWithVCService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new LWW_RegisterWithVCRepository();
                var service = new LWW_RegisterWithVCService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private bool CommutativeDownstreamAssign(Guid senderId, Guid objectId, JToken value, VectorClock clock, Dictionary<Node, LWW_RegisterWithVCService<TestType>> replicas)
        {
            var downstreamReplicas = replicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.DownstreamAssign(objectId, value, clock);
            }

            return true;
        }

        private void AssertExistsInRepository(TestType value, VectorClock vectorClock)
        {
            Assert.Equal(1, _repository.GetElements().Count(e => Equals(e.Value, value) &&
                                                            e.VectorClock.Equals(vectorClock)));
        }

        private void AssertDoesNotExistInRepository(TestType value, VectorClock vectorClock)
        {
            Assert.DoesNotContain(_repository.GetElements(),
                e => Equals(e.Value, value) &&
                     e.VectorClock.Equals(vectorClock));
        }
    }
}