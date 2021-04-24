using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Commutative;
using CRDT.Application.Entities;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Core.Cluster;
using CRDT.Registers.Operations;
using CRDT.UnitTestHelpers.TestTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using static CRDT.UnitTestHelpers.GuidHelpers;

namespace CRDT.Application.UnitTests.Commutative
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
            var operation = new Operation(JToken.Parse(JsonConvert.SerializeObject(value)), node, timestamp);

            _service.Update(value.Id, operation);

            AssertExistsInRepository(value, node, timestamp);
        }

        [Theory]
        [AutoData]
        public void Update_SameValueExistsWithLowerTimestamp_DoesNotDoAnything(TestType value, Node node, long timestamp)
        {
            _repository.AddValues(new PersistenceEntity<TestType>(value, node, timestamp - 100));

            var operation = new Operation(JToken.Parse(JsonConvert.SerializeObject(value)), node, timestamp);

            _service.Update(value.Id, operation);

            AssertExistsInRepository(value, node, timestamp - 100);
        }

        [Theory]
        [AutoData]
        public void Update_SameValueExistsWithHigherTimestamp_DoesNotDoAnything(TestType value, Node node, long timestamp)
        {
            _repository.AddValues(new PersistenceEntity<TestType>(value, node, timestamp));

            var operation = new Operation(JToken.Parse(JsonConvert.SerializeObject(value)), node, timestamp - 100);

            _service.Update(value.Id, operation);

            AssertExistsInRepository(value, node, timestamp);
        }

        [Theory]
        [AutoData]
        public void Update_DifferentValueExistsWithLowerTimestamp_ReplacesEntity(Guid id, Node node, long timestamp)
        {
            var value = TestTypeBuilder.Build(id);
            var newValue = TestTypeBuilder.Build(id);

            _repository.AddValues(new PersistenceEntity<TestType>(value, node, timestamp));

            var operation = new Operation(JToken.Parse(JsonConvert.SerializeObject(newValue)), node, timestamp + 100);

            _service.Update(value.Id, operation);

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

            var operation = new Operation(JToken.Parse(JsonConvert.SerializeObject(newValue)), node, timestamp);

            _service.Update(id, operation);

            AssertExistsInRepository(value, node, timestamp + 100);
            AssertDoesNotExistInRepository(newValue, node, timestamp);
        }

        [Theory]
        [AutoData]
        public void Update_Concurrent_AddsOnlyOneEntityWithLowerNodeValue(TestType value, long timestamp)
        {
            var nodeOne = new Node(GenerateGuid('a'));
            var nodeTwo = new Node(GenerateGuid('b'));

            var operationOne = new Operation(JToken.Parse(JsonConvert.SerializeObject(value)), nodeOne, timestamp);
            var operationTwo = new Operation(JToken.Parse(JsonConvert.SerializeObject(value)), nodeTwo, timestamp);

            _service.Update(value.Id, operationOne);
            _service.Update(value.Id, operationTwo);

            AssertExistsInRepository(value, nodeOne, timestamp);
        }

        [Theory]
        [AutoData]
        public void Update_PartOfState_BuildsSameValue(TestType value, Node node, long timestamp)
        {
            var operations = new List<Operation>
            {
                new(JToken.Parse($"{{\"StringValue\": \"{value.StringValue}\"}}"), node, timestamp),
                new(JToken.Parse($"{{\"IntValue\": {value.IntValue}}}"), node, timestamp + 1),
                new(JToken.Parse($"{{\"DecimalValue\": {value.DecimalValue}}}"), node, timestamp + 2),
                new(JToken.Parse($"{{\"NullableLongValue\": {value.NullableLongValue}}}"), node, timestamp + 3),
                new(JToken.Parse($"{{\"GuidValue\": \"{value.GuidValue}\"}}"), node, timestamp + 4),
                new(JToken.Parse($"{{\"IntArray\": {JsonConvert.SerializeObject(value.IntArray)}}}"), node, timestamp + 5),
                new(JToken.Parse($"{{\"LongList\": {JsonConvert.SerializeObject(value.LongList)}}}"), node, timestamp + 6),
                new(JToken.Parse($"{{\"ObjectValue\": {JsonConvert.SerializeObject(value.ObjectValue)}}}"), node, timestamp + 7),
            };

            foreach (var operation in operations)
            {
                _service.Update(value.Id, operation);
            }

            AssertExistsInRepository(value, node, timestamp + 7);
        }

        [Theory]
        [AutoData]
        public void Update_MixedTimestamps_BuildsCorrectValue(TestType value, Node node, long timestamp)
        {
            var operations = new List<Operation>
            {
                new(JToken.Parse($"{{\"StringValue\": \"{value.StringValue}\"}}"), node, timestamp),
                new(JToken.Parse($"{{\"IntValue\": 999}}"), node, timestamp + 1),
                new(JToken.Parse($"{{\"IntArray\": null}}"), node, timestamp + 3),
                new(JToken.Parse($"{{\"DecimalValue\": {value.DecimalValue}}}"), node, timestamp + 2),
                new(JToken.Parse($"{{\"NullableLongValue\": {value.NullableLongValue}}}"), node, timestamp + 3),
                new(JToken.Parse($"{{\"NotExistingValue\": {value.NullableLongValue}}}"), node, timestamp + 3),
                new(JToken.Parse($"{{\"GuidValue\": \"{value.GuidValue}\"}}"), node, timestamp + 4),
                new(JToken.Parse($"{{\"IntArray\": {JsonConvert.SerializeObject(value.IntArray)}}}"), node, timestamp + 5),
                new(JToken.Parse($"{{\"LongList\": {JsonConvert.SerializeObject(value.LongList)}}}"), node, timestamp + 6),
                new(JToken.Parse($"{{\"IntValue\": {value.IntValue}}}"), node, timestamp + 7),
                new(JToken.Parse($"{{\"ObjectValue\": {JsonConvert.SerializeObject(value.ObjectValue)}}}"), node, timestamp + 7),
                new(JToken.Parse($"{{\"DecimalValue\": {value.DecimalValue}}}"), node, timestamp + 7),
            };

            foreach (var operation in operations)
            {
                _service.Update(value.Id, operation);
            }

            AssertExistsInRepository(value, node, timestamp + 7);
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
            var operation = new Operation(JToken.Parse(JsonConvert.SerializeObject(value)), node, timestamp);

            _service.Update(value.Id, operation);

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