using System;
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

            _service.Assign(value.Id, value, new VectorClock(clock.Add(node, 0)));

            AssertExistsInRepository(value, new VectorClock(clock.Add(node, 0)));
        }

        [Theory]
        [AutoData]
        public void Assign_SameValueExistsWithLowerTimestamp_DoesNotDoAnything(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistElement(new LWW_RegisterWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0))));

            _service.Assign(value.Id, value, new VectorClock(clock.Add(node, 1)));

            AssertExistsInRepository(value, new VectorClock(clock.Add(node, 0)));
        }

        [Theory]
        [AutoData]
        public void Assign_SameValueExistsWithHigherTimestamp_DoesNotDoAnything(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistElement(new LWW_RegisterWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1))));

            _service.Assign(value.Id, value, new VectorClock(clock.Add(node, 0)));

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

            _service.Assign(id, newValue, new VectorClock(clock.Add(node, 1)));

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

            _service.Assign(value.Id, newValue, new VectorClock(clock.Add(node, 0)));

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

            _service.Assign(value.Id, value, new VectorClock(clock.Add(node, 0)));

            var actualValue = _service.GetValue(value.Id);

            Assert.Equal(value, actualValue);
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