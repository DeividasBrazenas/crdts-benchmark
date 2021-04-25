using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Convergent;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Application.UnitTests.Convergent
{
    public class LWW_SetServiceTests
    {
        private readonly ILWW_SetRepository<TestType> _repository;
        private readonly LWW_SetService<TestType> _lwwSetService;

        public LWW_SetServiceTests()
        {
            _repository = new LWW_SetRepository();
            _lwwSetService = new LWW_SetService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void Add_NoExistingValues_AddsElementToTheRepository(LWW_SetElement<TestType> element)
        {
            _lwwSetService.Merge(new List<LWW_SetElement<TestType>> { element }, new List<LWW_SetElement<TestType>>());

            var repositoryValues = _repository.GetAdds();
            Assert.Contains(element, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(List<LWW_SetElement<TestType>> adds, LWW_SetElement<TestType> element)
        {
            _repository.PersistAdds(adds);

            _lwwSetService.Merge(new List<LWW_SetElement<TestType>> { element }, new List<LWW_SetElement<TestType>>());

            var repositoryValues = _repository.GetAdds();
            Assert.Contains(element, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddDoesNotExist_DoesNotAddElementToTheRepository(LWW_SetElement<TestType> element)
        {
            _lwwSetService.Merge(new List<LWW_SetElement<TestType>>(), new List<LWW_SetElement<TestType>> { element });

            var repositoryValues = _repository.GetRemoves();
            Assert.DoesNotContain(element, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddExistsWithLowerTimestamp_AddsElementToTheRepository(TestType value, long timestamp)
        {
            var addElement = new LWW_SetElement<TestType>(value, timestamp);
            var removeElement = new LWW_SetElement<TestType>(value, timestamp + 10);

            _lwwSetService.Merge(new List<LWW_SetElement<TestType>> { addElement }, new List<LWW_SetElement<TestType>> { removeElement });

            var repositoryValues = _repository.GetRemoves();
            Assert.Contains(value, repositoryValues.Select(v => v.Value));
        }

        [Theory]
        [AutoData]
        public void Merge_NoExistingValues_AddsElementsToTheRepository(List<LWW_SetElement<TestType>> values)
        {
            _lwwSetService.Merge(values, values);

            var repositoryAdds = _repository.GetAdds();
            var repositoryRemoves = _repository.GetRemoves();

            AssertContains(values, repositoryAdds);
            AssertContains(values, repositoryRemoves);
        }

        [Theory]
        [AutoData]
        public void Merge_WithExistingValues_AddsElementsToTheRepository(List<LWW_SetElement<TestType>> existingAdds, List<LWW_SetElement<TestType>> existingRemoves, List<LWW_SetElement<TestType>> values)
        {
            _repository.PersistAdds(existingAdds);
            _repository.PersistRemoves(existingRemoves);

            _lwwSetService.Merge(values, values);

            var repositoryAdds = _repository.GetAdds();
            var repositoryRemoves = _repository.GetRemoves();

            AssertContains(values, repositoryAdds);
            AssertContains(values, repositoryRemoves);
        }

        [Theory]
        [AutoData]
        public void Merge_IsIdempotent(List<LWW_SetElement<TestType>> existingAdds, List<LWW_SetElement<TestType>> existingRemoves, List<LWW_SetElement<TestType>> values)
        {
            _repository.PersistAdds(existingAdds);
            _repository.PersistRemoves(existingRemoves);

            _lwwSetService.Merge(values, values);
            _lwwSetService.Merge(values, values);
            _lwwSetService.Merge(values, values);

            var repositoryAdds = _repository.GetAdds();
            var repositoryRemoves = _repository.GetRemoves();

            AssertContains(values, repositoryAdds);
            AssertContains(values, repositoryRemoves);
        }

        [Theory]
        [AutoData]
        public void Lookup_Added_ReturnsTrue(List<LWW_SetElement<TestType>> existingAdds, List<LWW_SetElement<TestType>> existingRemoves, LWW_SetElement<TestType> element)
        {
            _repository.PersistAdds(existingAdds);
            _repository.PersistRemoves(existingRemoves);

            _lwwSetService.Merge(new List<LWW_SetElement<TestType>> { element }, new List<LWW_SetElement<TestType>>());

            var lookup = _lwwSetService.Lookup(element.Value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_Removed_ReturnsFalse(List<LWW_SetElement<TestType>> existingAdds, List<LWW_SetElement<TestType>> existingRemoves, TestType value, long timestamp)
        {
            _repository.PersistAdds(existingAdds);
            _repository.PersistRemoves(existingRemoves);

            var addElement = new LWW_SetElement<TestType>(value, timestamp);
            var removeElement = new LWW_SetElement<TestType>(value, timestamp + 100);

            _lwwSetService.Merge(new List<LWW_SetElement<TestType>> { addElement }, new List<LWW_SetElement<TestType>> { removeElement });

            var lookup = _lwwSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsTrue(List<LWW_SetElement<TestType>> existingAdds, List<LWW_SetElement<TestType>> existingRemoves, TestType value, long timestamp)
        {
            _repository.PersistAdds(existingAdds);
            _repository.PersistRemoves(existingRemoves);

            var addElement = new LWW_SetElement<TestType>(value, timestamp);
            var removeElement = new LWW_SetElement<TestType>(value, timestamp + 100);
            var reAddElement = new LWW_SetElement<TestType>(value, timestamp + 200);

            _lwwSetService.Merge(new List<LWW_SetElement<TestType>> { addElement }, new List<LWW_SetElement<TestType>> { removeElement });

            var lookup = _lwwSetService.Lookup(value);

            Assert.False(lookup);

            _lwwSetService.Merge(new List<LWW_SetElement<TestType>> { reAddElement }, new List<LWW_SetElement<TestType>>());

            lookup = _lwwSetService.Lookup(value);

            Assert.True(lookup);
        }

        private void AssertContains(List<LWW_SetElement<TestType>> expectedValues, IEnumerable<LWW_SetElement<TestType>> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}