using System;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Convergent;
using CRDT.Application.Entities;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Core.Cluster;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Application.UnitTests.Convergent
{
    public class LWW_RegisterServiceTests
    {
        private readonly LWW_RegisterService<TestType> _service;
        private readonly TestTypeRepository _repository;

        public LWW_RegisterServiceTests()
        {
            _repository = new TestTypeRepository();
            _service = new LWW_RegisterService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void Update_EmptyRepository_AddsToRepository(TestType value, Node node, long timestamp)
        {
            _service.Update(value.Id, value, node, timestamp);

            AssertExistsInRepository(value, node, timestamp);
        }

        [Theory]
        [AutoData]
        public void Update_SameValueExistsWithLowerTimestamp_DoesNotDoAnything(TestType value, Node node, long timestamp)
        {
            _repository.AddValues(new PersistenceEntity<TestType>(value, node, timestamp - 100));

            _service.Update(value.Id, value, node, timestamp);

            AssertExistsInRepository(value, node, timestamp - 100);
        }

        [Theory]
        [AutoData]
        public void Update_SameValueExistsWithHigherTimestamp_DoesNotDoAnything(TestType value, Node node, long timestamp)
        {
            _repository.AddValues(new PersistenceEntity<TestType>(value, node, timestamp));

            _service.Update(value.Id, value, node, timestamp - 100);

            AssertExistsInRepository(value, node, timestamp);
        }

        [Theory]
        [AutoData]
        public void Update_DifferentValueExistsWithLowerTimestamp_ReplacesEntity(Guid id, Node node, long timestamp)
        {
            var value = TestTypeBuilder.Build(id);
            var newValue = TestTypeBuilder.Build(id);

            _repository.AddValues(new PersistenceEntity<TestType>(value, node, timestamp));

            _service.Update(id, newValue, node, timestamp + 100);

            AssertDoesNotExistInRepository(value, node, timestamp);
            AssertExistsInRepository(newValue, node, timestamp + 100);
        }

        [Theory]
        [AutoData]
        public void Update_DifferentValueExistsWithHigherTimestamp_DoesNotDoAnything(Guid id, Node node, long timestamp)
        {
            var value = TestTypeBuilder.Build(id);
            var newValue = TestTypeBuilder.Build(id);

            _repository.AddValues(new PersistenceEntity<TestType>(value, node, timestamp + 100));

            _service.Update(value.Id, newValue, node, timestamp);

            AssertExistsInRepository(value, node, timestamp + 100);
            AssertDoesNotExistInRepository(newValue, node, timestamp);
        }

        [Fact]
        public void Value_NotExistingEntity_ReturnsNull()
        {
            var value = _service.GetValue(Guid.NewGuid());

            Assert.Null(value);
        }

        [Theory]
        [AutoData]
        public void Value_ExistingEntity_ReturnsValue(TestType value, Node node, long timestamp)
        {
            _service.Update(value.Id, value, node, timestamp);

            var actualValue = _service.GetValue(value.Id);

            Assert.Equal(value, actualValue);
        }

        private void AssertExistsInRepository(TestType value, Node updatedBy, long timestamp)
        {
            Assert.Equal(1, _repository.Entities.Count(e => Equals(e.Value, value) &&
                                                            Equals(e.UpdatedBy, updatedBy) &&
                                                            e.Timestamp.Value == timestamp));
        }

        private void AssertDoesNotExistInRepository(TestType value, Node updatedBy, long timestamp)
        {
            Assert.DoesNotContain(_repository.Entities,
                e => Equals(e.Value, value) &&
                     Equals(e.UpdatedBy, updatedBy) &&
                     e.Timestamp.Value == timestamp);
        }
    }
}