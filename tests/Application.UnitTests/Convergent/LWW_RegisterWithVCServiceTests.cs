using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Convergent.Register;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using CRDT.Registers.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Application.UnitTests.Convergent
{
    public class LWW_RegisterWithVCServiceTests
    {
        private readonly LWW_RegisterWithVCService<TestType> _service;
        private readonly ILWW_RegisterWithVCRepository<TestType> _repository;

        public LWW_RegisterWithVCServiceTests()
        {
            _repository = new LWW_RegisterWithVCRepository();
            _service = new LWW_RegisterWithVCService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void Assign_EmptyRepository_AddsToRepository(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _service.DownstreamAssign(value.Id, value, new VectorClock(clock.Add(node, 0)));

            AssertExistsInRepository(value, new VectorClock(clock.Add(node, 0)));
        }

        [Theory]
        [AutoData]
        public void Assign_SameValueExistsWithLowerTimestamp_DoesNotDoAnything(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistElement(new LWW_RegisterWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0))));

            _service.DownstreamAssign(value.Id, value, new VectorClock(clock.Add(node, 1)));

            AssertExistsInRepository(value, new VectorClock(clock.Add(node, 0)));
        }

        [Theory]
        [AutoData]
        public void Assign_SameValueExistsWithHigherTimestamp_DoesNotDoAnything(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistElement(new LWW_RegisterWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1))));

            _service.DownstreamAssign(value.Id, value, new VectorClock(clock.Add(node, 0)));

            AssertExistsInRepository(value, new VectorClock(clock.Add(node, 1)));
        }

        [Theory]
        [AutoData]
        public void Assign_DifferentValueExistsWithLowerTimestamp_ReplacesEntity(Guid id, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var value = TestTypeBuilder.Build(id);
            var newValue = TestTypeBuilder.Build(id);

            _repository.PersistElement(new LWW_RegisterWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0))));

            _service.DownstreamAssign(id, newValue, new VectorClock(clock.Add(node, 1)));

            AssertDoesNotExistInRepository(value, new VectorClock(clock.Add(node, 0)));
            AssertExistsInRepository(newValue, new VectorClock(clock.Add(node, 1)));
        }

        [Theory]
        [AutoData]
        public void Assign_DifferentValueExistsWithHigherTimestamp_DoesNotDoAnything(Guid id, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var value = TestTypeBuilder.Build(id);
            var newValue = TestTypeBuilder.Build(id);

            _repository.PersistElement(new LWW_RegisterWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1))));

            _service.DownstreamAssign(value.Id, newValue, new VectorClock(clock.Add(node, 0)));

            AssertExistsInRepository(value, new VectorClock(clock.Add(node, 1)));
            AssertDoesNotExistInRepository(newValue, new VectorClock(clock.Add(node, 0)));
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

            _service.DownstreamAssign(value.Id, value, new VectorClock(clock.Add(node, 0)));

            var actualValue = _service.GetValue(value.Id);

            Assert.Equal(value, actualValue);
        }

        [Fact]
        public void Convergent_Assign_NewValue()
        {
            var nodes = CreateNodes(3);
            var convergentReplicas = CreateConvergentReplicas(nodes);

            var initialValue = TestTypeBuilder.Build();
            var valueId = initialValue.Id;

            var clock = new VectorClock(nodes);

            var firstReplica = convergentReplicas.First();
            firstReplica.Value.LocalAssign(valueId, initialValue, clock);

            ConvergentDownstreamAssign(firstReplica.Key.Id, firstReplica.Value.GetValue(valueId), clock, convergentReplicas);

            clock.Increment(firstReplica.Key);

            foreach (var replica in convergentReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    var newValue = TestTypeBuilder.Build(valueId);

                    replica.Value.LocalAssign(valueId, newValue, clock);

                    ConvergentDownstreamAssign(replica.Key.Id, replica.Value.GetValue(valueId), clock, convergentReplicas);

                    clock.Increment(replica.Key);
                }
            }

            foreach (var replica in convergentReplicas)
            {
                Assert.Equal(initialValue, replica.Value.GetValue(valueId));
            }
        }

        [Fact]
        public void Convergent_Assign_UpdateSingleField()
        {
            var nodes = CreateNodes(3);
            var convergentReplicas = CreateConvergentReplicas(nodes);

            var initialValue = TestTypeBuilder.Build();
            var valueId = initialValue.Id;

            var clock = new VectorClock(nodes);

            var firstReplica = convergentReplicas.First();
            firstReplica.Value.LocalAssign(valueId, initialValue, clock);

            ConvergentDownstreamAssign(firstReplica.Key.Id, firstReplica.Value.GetValue(valueId), clock, convergentReplicas);

            clock.Increment(firstReplica.Key);

            foreach (var replica in convergentReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    initialValue.StringValue = Guid.NewGuid().ToString();

                    replica.Value.LocalAssign(valueId, initialValue, clock);

                    ConvergentDownstreamAssign(replica.Key.Id, replica.Value.GetValue(valueId), clock, convergentReplicas);

                    clock.Increment(replica.Key);
                }
            }

            foreach (var replica in convergentReplicas)
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

        private Dictionary<Node, LWW_RegisterWithVCService<TestType>> CreateConvergentReplicas(List<Node> nodes)
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

        private bool ConvergentDownstreamAssign(Guid senderId, TestType state, VectorClock clock, Dictionary<Node, LWW_RegisterWithVCService<TestType>> replicas)
        {
            var downstreamReplicas = replicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.DownstreamAssign(senderId, state, clock);
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