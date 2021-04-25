using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Commutative;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Core.Cluster;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;
using static CRDT.UnitTestHelpers.TestTypes.TestTypeBuilder;

namespace CRDT.Application.UnitTests.Commutative
{
    public class OUR_SetServiceTests
    {
        private readonly IOUR_SetRepository<TestType> _repository;
        private readonly OUR_SetService<TestType> _ourSetService;

        public OUR_SetServiceTests()
        {
            _repository = new OUR_SetRepository();
            _ourSetService = new OUR_SetService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void Add_NoExistingValues_AddsElementToTheRepository(TestType value, Node node, long timestamp)
        {
            _ourSetService.Add(value, node, timestamp);

            var repositoryValues = _repository.GetAdds();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == node.Id && v.Timestamp.Value == timestamp);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(List<OUR_SetElement<TestType>> adds, TestType value, Node node, long timestamp)
        {
            _repository.PersistAdds(adds);

            _ourSetService.Add(value, node, timestamp);

            var repositoryValues = _repository.GetAdds();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == node.Id && v.Timestamp.Value == timestamp);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithDifferentTag_AddsElementToTheRepository(List<OUR_SetElement<TestType>> adds, TestType value, Node node, Node otherNode, long timestamp)
        {
            _repository.PersistAdds(adds);

            _ourSetService.Add(value, node, timestamp);
            _ourSetService.Add(value, otherNode, timestamp);

            var repositoryValues = _repository.GetAdds();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Equal(2, actualValues.Count());
        }

        [Theory]
        [AutoData]
        public void Add_IsIdempotent(TestType value, Node node, long timestamp)
        {
            _ourSetService.Add(value, node, timestamp);
            _ourSetService.Add(value, node, timestamp);
            _ourSetService.Add(value, node, timestamp);

            var repositoryValues = _repository.GetAdds();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddDoesNotExist_DoesNotAddElementToTheRepository(TestType value, Node node, long timestamp)
        {
            _ourSetService.Remove(value, new[] { node.Id }, timestamp);

            var repositoryValues = _repository.GetRemoves();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Empty(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddExists_AddsElementToTheRepository(TestType value, Node node, long timestamp)
        {
            _ourSetService.Add(value, node, timestamp);
            _ourSetService.Remove(value, new[] { node.Id }, timestamp);

            var repositoryValues = _repository.GetRemoves();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == node.Id);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_IsIdempotent(TestType value, Node node, long timestamp)
        {
            _ourSetService.Add(value, node, timestamp);
            _ourSetService.Remove(value, new[] { node.Id }, timestamp);
            _ourSetService.Remove(value, new[] { node.Id }, timestamp);
            _ourSetService.Remove(value, new[] { node.Id }, timestamp);

            var repositoryValues = _repository.GetRemoves();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == node.Id);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Lookup_SingleElementAdded_ReturnsTrue(TestType value, Node node, long timestamp)
        {
            _ourSetService.Add(value, node, timestamp);

            var lookup = _ourSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SeveralElementsWithDifferentTags_ReturnsTrue(TestType value, Node node, Node otherNode, long timestamp)
        {
            _ourSetService.Add(value, node, timestamp);
            _ourSetService.Add(value, otherNode, timestamp + 5);

            var lookup = _ourSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_NonExistingElement_ReturnsFalse(TestType value)
        {
            var lookup = _ourSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AllTagsRemoved_ReturnsFalse(TestType value, Node node, Node otherNode, long timestamp)
        {
            _ourSetService.Add(value, node, timestamp);
            _ourSetService.Add(value, otherNode, timestamp);
            _ourSetService.Remove(value, new []{node.Id, otherNode.Id}, timestamp);

            var lookup = _ourSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_UpdatedElement_ReturnsTrueForUpdatedElement(TestType value, Node node, long timestamp)
        {
            _ourSetService.Add(value, node, timestamp);

            var newValue = Build(value.Id);
            _ourSetService.Update(newValue, node, timestamp + 3);

            var lookup = _ourSetService.Lookup(newValue);
            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_UpdatedElement_ReturnsFalseForOldElement(TestType value, Node node, long timestamp)
        {
            _ourSetService.Add(value, node, timestamp);

            var newValue = Build(value.Id);
            _ourSetService.Update(newValue, node, timestamp + 3);

            var lookup = _ourSetService.Lookup(value);
            Assert.False(lookup);
        }
    }
}