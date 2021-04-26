using System;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Convergent;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Registers.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Application.UnitTests.Convergent
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
        public void Assign_EmptyRepository_AddsToRepository(TestType value, long timestamp)
        {
            _service.Assign(value.Id, value, timestamp);

            AssertExistsInRepository(value, timestamp);
        }

        [Theory]
        [AutoData]
        public void Assign_SameValueExistsWithLowerTimestamp_DoesNotDoAnything(TestType value, long timestamp)
        {
            _repository.PersistElement(new LWW_RegisterElement<TestType>(value, timestamp - 100));

            _service.Assign(value.Id, value, timestamp);

            AssertExistsInRepository(value, timestamp - 100);
        }

        [Theory]
        [AutoData]
        public void Assign_SameValueExistsWithHigherTimestamp_DoesNotDoAnything(TestType value, long timestamp)
        {
            _repository.PersistElement(new LWW_RegisterElement<TestType>(value, timestamp));

            _service.Assign(value.Id, value, timestamp - 100);

            AssertExistsInRepository(value, timestamp);
        }

        [Theory]
        [AutoData]
        public void Assign_DifferentValueExistsWithLowerTimestamp_ReplacesEntity(Guid id, long timestamp)
        {
            var value = TestTypeBuilder.Build(id);
            var newValue = TestTypeBuilder.Build(id);

            _repository.PersistElement(new LWW_RegisterElement<TestType>(value, timestamp));

            _service.Assign(id, newValue, timestamp + 100);

            AssertDoesNotExistInRepository(value, timestamp);
            AssertExistsInRepository(newValue, timestamp + 100);
        }

        [Theory]
        [AutoData]
        public void Assign_DifferentValueExistsWithHigherTimestamp_DoesNotDoAnything(Guid id, long timestamp)
        {
            var value = TestTypeBuilder.Build(id);
            var newValue = TestTypeBuilder.Build(id);

            _repository.PersistElement(new LWW_RegisterElement<TestType>(value, timestamp + 100));

            _service.Assign(value.Id, newValue, timestamp);

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
            _service.Assign(value.Id, value, timestamp);

            var actualValue = _service.GetValue(value.Id);

            Assert.Equal(value, actualValue);
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