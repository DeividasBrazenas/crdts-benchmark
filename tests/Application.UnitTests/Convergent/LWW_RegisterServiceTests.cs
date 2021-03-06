using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Convergent.Register;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Core.Cluster;
using CRDT.Registers.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Application.UnitTests.Convergent
{
    public class LWW_RegisterServiceTests
    {
        private readonly LWW_RegisterService<TestType> _service;
        private readonly ILWW_RegisterRepository<TestType> _repository;
        private readonly TestTypeBuilder _builder;

        public LWW_RegisterServiceTests()
        {
            _repository = new LWW_RegisterRepository();
            _service = new LWW_RegisterService<TestType>(_repository);
            _builder = new TestTypeBuilder(new Random());
        }

        [Theory]
        [AutoData]
        public void Assign_EmptyRepository_AddsToRepository(TestType value, long timestamp)
        {
            _service.DownstreamAssign(value, timestamp);

            AssertExistsInRepository(value, timestamp);
        }

        [Theory]
        [AutoData]
        public void Assign_SameValueExistsWithHigherTimestamp_DoesNotDoAnything(TestType value, long timestamp)
        {
            _repository.PersistElement(new LWW_RegisterElement<TestType>(value, timestamp, false));

            _service.DownstreamAssign(value, timestamp - 100);

            AssertExistsInRepository(value, timestamp);
        }

        [Theory]
        [AutoData]
        public void Assign_DifferentValueExistsWithLowerTimestamp_ReplacesEntity(Guid id, long timestamp)
        {
            var value = _builder.Build(id);
            var newValue = _builder.Build(id);

            _repository.PersistElement(new LWW_RegisterElement<TestType>(value, timestamp, false));

            _service.DownstreamAssign(newValue, timestamp + 100);

            AssertDoesNotExistInRepository(value, timestamp);
            AssertExistsInRepository(newValue, timestamp + 100);
        }

        [Theory]
        [AutoData]
        public void Assign_DifferentValueExistsWithHigherTimestamp_DoesNotDoAnything(Guid id, long timestamp)
        {
            var value = _builder.Build(id);
            var newValue = _builder.Build(id);

            _repository.PersistElement(new LWW_RegisterElement<TestType>(value, timestamp + 100, false));

            _service.DownstreamAssign(newValue, timestamp);

            AssertExistsInRepository(value, timestamp + 100);
            AssertDoesNotExistInRepository(newValue, timestamp);
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
            _service.DownstreamAssign(value, timestamp);

            var actualValue = _service.GetValue(value.Id);

            Assert.Equal(value, actualValue.Value);
        }

        [Fact]
        public void Convergent_Assign_UpdateSingleField()
        {
            var nodes = CreateNodes(3);
            var convergentReplicas = CreateConvergentReplicas(nodes);

            var initialValue = _builder.Build();
            var valueId = initialValue.Id;

            long ts = 1;

            var firstReplica = convergentReplicas.First();
            firstReplica.Value.LocalAssign(initialValue, ts);

            ConvergentDownstreamAssign(firstReplica.Key.Id, firstReplica.Value.GetValue(valueId).Value, ts, convergentReplicas);

            ts++;

            foreach (var replica in convergentReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    initialValue.StringValue = Guid.NewGuid().ToString();

                    replica.Value.LocalAssign(initialValue, ts);

                    ConvergentDownstreamAssign(replica.Key.Id, replica.Value.GetValue(valueId).Value, ts, convergentReplicas);

                    ts++;
                }
            }

            foreach (var replica in convergentReplicas)
            {
                Assert.Equal(initialValue, replica.Value.GetValue(valueId).Value);
            }
        }

        private void AssertExistsInRepository(TestType value, long timestamp)
        {
            Assert.Equal(1, _repository.GetElements().Count(e => Equals(e.Value, value) &&
                                                                 e.Timestamp == timestamp));
        }

        private void AssertDoesNotExistInRepository(TestType value, long timestamp)
        {
            Assert.DoesNotContain(_repository.GetElements(),
                e => Equals(e.Value, value) &&
                     e.Timestamp == timestamp);
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

        private Dictionary<Node, LWW_RegisterService<TestType>> CreateConvergentReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Convergent.Register.LWW_RegisterService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new LWW_RegisterRepository();
                var service = new CRDT.Application.Convergent.Register.LWW_RegisterService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void ConvergentDownstreamAssign(Guid senderId, TestType state, long timestamp, Dictionary<Node, LWW_RegisterService<TestType>> replicas)
        {
            var downstreamReplicas = replicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.DownstreamAssign(state, timestamp);
            }
        }

    }
}