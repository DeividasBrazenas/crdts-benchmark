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

namespace CRDT.Application.UnitTests.Commutative
{
    public class OR_OptimizedSetServiceTests
    {
        private readonly IOR_OptimizedSetRepository<TestType> _repository;
        private readonly OR_OptimizedSetService<TestType> _orSetService;

        public OR_OptimizedSetServiceTests()
        {
            _repository = new OR_OptimizedSetRepository();
            _orSetService = new OR_OptimizedSetService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void Add_NoExistingValues_AddsElementToTheRepository(TestType value, Node node)
        {
            _orSetService.Add(value, node);

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == node.Id);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(List<OR_OptimizedSetElement<TestType>> adds, TestType value, Node node)
        {
            _repository.PersistElements(adds);

            _orSetService.Add(value, node);

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == node.Id);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithDifferentTag_AddsElementToTheRepository(List<OR_OptimizedSetElement<TestType>> adds, TestType value, Node node, Node otherNode)
        {
            _repository.PersistElements(adds);

            _orSetService.Add(value, node);
            _orSetService.Add(value, otherNode);

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Equal(2, actualValues.Count());
        }

        [Theory]
        [AutoData]
        public void Add_IsIdempotent(TestType value, Node node)
        {
            _orSetService.Add(value, node);
            _orSetService.Add(value, node);
            _orSetService.Add(value, node);

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddDoesNotExist_DoesNotAddElementToTheRepository(TestType value, Node node)
        {
            _orSetService.Remove(value, new[] { node.Id });

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Empty(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddExists_AddsElementToTheRepository(TestType value, Node node)
        {
            _orSetService.Add(value, node);
            _orSetService.Remove(value, new[] { node.Id });

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == node.Id);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_IsIdempotent(TestType value, Node node)
        {
            _orSetService.Add(value, node);
            _orSetService.Remove(value, new[] { node.Id });
            _orSetService.Remove(value, new[] { node.Id });
            _orSetService.Remove(value, new[] { node.Id });

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == node.Id);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Lookup_SingleElementAdded_ReturnsTrue(TestType value, Node node)
        {
            _orSetService.Add(value, node);

            var lookup = _orSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SeveralElementsWithDifferentTags_ReturnsTrue(TestType value, Node node, Node otherNode)
        {
            _orSetService.Add(value, node);
            _orSetService.Add(value, otherNode);

            var lookup = _orSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_NonExistingElement_ReturnsFalse(TestType value)
        {
            var lookup = _orSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AllTagsRemoved_ReturnsFalse(TestType value, Node node, Node otherNode)
        {
            _orSetService.Add(value, node);
            _orSetService.Add(value, otherNode);
            _orSetService.Remove(value, new []{node.Id, otherNode.Id});

            var lookup = _orSetService.Lookup(value);

            Assert.False(lookup);
        }
    }
}