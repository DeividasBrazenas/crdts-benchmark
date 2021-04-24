using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Convergent;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Core.Cluster;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Application.UnitTests.Convergent
{
    public class OR_SetServiceTests
    {
        private readonly IOR_SetRepository<TestType> _repository;
        private readonly OR_SetService<TestType> _orSetService;

        public OR_SetServiceTests()
        {
            _repository = new OR_SetRepository();
            _orSetService = new OR_SetService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void Add_NoExistingValues_AddsElementToTheRepository(TestType value, Node node)
        {
            _orSetService.Add(value, node);

            var repositoryValues = _repository.GetAdds();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == node.Id);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(List<OR_SetElement<TestType>> adds, TestType value, Node node)
        {
            _repository.PersistAdds(adds);

            _orSetService.Add(value, node);

            var repositoryValues = _repository.GetAdds();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == node.Id);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithDifferentTag_AddsElementToTheRepository(List<OR_SetElement<TestType>> adds, TestType value, Node node, Node otherNode)
        {
            _repository.PersistAdds(adds);

            _orSetService.Add(value, node);
            _orSetService.Add(value, otherNode);

            var repositoryValues = _repository.GetAdds();
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

            var repositoryValues = _repository.GetAdds();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddDoesNotExist_DoesNotAddElementToTheRepository(TestType value, Node node)
        {
            _orSetService.Remove(value, node);

            var repositoryValues = _repository.GetRemoves();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Empty(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddExists_AddsElementToTheRepository(TestType value, Node node)
        {
            _orSetService.Add(value, node);
            _orSetService.Remove(value, node);

            var repositoryValues = _repository.GetRemoves();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == node.Id);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_IsIdempotent(TestType value, Node node)
        {
            _orSetService.Add(value, node);
            _orSetService.Remove(value, node);
            _orSetService.Remove(value, node);
            _orSetService.Remove(value, node);

            var repositoryValues = _repository.GetRemoves();
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
            _orSetService.Remove(value, node);
            _orSetService.Remove(value, otherNode);

            var lookup = _orSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Merge_NoExistingValues_AddsElementsToTheRepository(List<OR_SetElement<TestType>> adds, List<OR_SetElement<TestType>> removes)
        {
            _orSetService.Merge(adds, removes);

            var repositoryAdds = _repository.GetAdds();
            var repositoryRemoves = _repository.GetRemoves();

            AssertContains(adds, repositoryAdds);
            AssertContains(removes, repositoryRemoves);
        }

        [Theory]
        [AutoData]
        public void Merge_WithExistingValues_AddsElementsToTheRepository(List<OR_SetElement<TestType>> existingAdds, List<OR_SetElement<TestType>> existingRemoves, List<OR_SetElement<TestType>> adds, List<OR_SetElement<TestType>> removes)
        {
            _repository.PersistAdds(existingAdds);
            _repository.PersistRemoves(existingRemoves);

            _orSetService.Merge(adds, removes);

            var repositoryAdds = _repository.GetAdds();
            var repositoryRemoves = _repository.GetRemoves();

            AssertContains(adds, repositoryAdds);
            AssertContains(removes, repositoryRemoves);
        }

        [Theory]
        [AutoData]
        public void Merge_IsIdempotent(List<OR_SetElement<TestType>> existingAdds, List<OR_SetElement<TestType>> existingRemoves, List<OR_SetElement<TestType>> adds, List<OR_SetElement<TestType>> removes)
        {
            _repository.PersistAdds(existingAdds);
            _repository.PersistRemoves(existingRemoves);

            _orSetService.Merge(adds, removes);
            _orSetService.Merge(adds, removes);
            _orSetService.Merge(adds, removes);

            var repositoryAdds = _repository.GetAdds();
            var repositoryRemoves = _repository.GetRemoves();

            AssertContains(adds, repositoryAdds);
            AssertContains(removes, repositoryRemoves);
        }

        private void AssertContains(List<OR_SetElement<TestType>> expectedValues, IEnumerable<OR_SetElement<TestType>> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}