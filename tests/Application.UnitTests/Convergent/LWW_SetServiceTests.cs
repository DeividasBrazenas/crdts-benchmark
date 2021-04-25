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

        //[Theory]
        //[AutoData]
        //public void Add_NoExistingValues_AddsElementToTheRepository(TestType value, long timestamp)
        //{
        //    _lwwSetService.Add(value, timestamp);

        //    var repositoryValues = _repository.GetAdds();
        //    Assert.Contains(value, repositoryValues.Select(v => v.Value));
        //}

        //[Theory]
        //[AutoData]
        //public void Add_WithExistingValues_AddsElementToTheRepository(List<LWW_SetElement<TestType>> adds, TestType value, long timestamp)
        //{
        //    _repository.PersistAdds(adds);

        //    _lwwSetService.Add(value, timestamp);

        //    var repositoryValues = _repository.GetAdds();
        //    Assert.Contains(value, repositoryValues.Select(v => v.Value));
        //}

        //[Theory]
        //[AutoData]
        //public void Remove_AddDoesNotExist_DoesNotAddElementToTheRepository(TestType value, long timestamp)
        //{
        //    _lwwSetService.Remove(value, timestamp);

        //    var repositoryValues = _repository.GetRemoves();
        //    Assert.DoesNotContain(value, repositoryValues.Select(v => v.Value));
        //}

        //[Theory]
        //[AutoData]
        //public void Remove_AddExistsWithLowerTimestamp_AddsElementToTheRepository(TestType value, long timestamp)
        //{
        //    _lwwSetService.Add(value, timestamp);
        //    _lwwSetService.Remove(value, timestamp + 10);

        //    var repositoryValues = _repository.GetRemoves();
        //    Assert.Contains(value, repositoryValues.Select(v => v.Value));
        //}

        [Theory]
        [AutoData]
        public void Merge_NoExistingValues_AddsElementsToTheRepository(List<LWW_SetElement<TestType>> adds, List<LWW_SetElement<TestType>> removes)
        {
            _lwwSetService.Merge(adds, removes);

            var repositoryAdds = _repository.GetAdds();
            var repositoryRemoves = _repository.GetRemoves();

            AssertContains(adds, repositoryAdds);
            AssertContains(removes, repositoryRemoves);
        }

        [Theory]
        [AutoData]
        public void Merge_WithExistingValues_AddsElementsToTheRepository(List<LWW_SetElement<TestType>> existingAdds, List<LWW_SetElement<TestType>> existingRemoves, List<LWW_SetElement<TestType>> adds, List<LWW_SetElement<TestType>> removes)
        {
            _repository.PersistAdds(existingAdds);
            _repository.PersistRemoves(existingRemoves);

            _lwwSetService.Merge(adds, removes);

            var repositoryAdds = _repository.GetAdds();
            var repositoryRemoves = _repository.GetRemoves();

            AssertContains(adds, repositoryAdds);
            AssertContains(removes, repositoryRemoves);
        }

        [Theory]
        [AutoData]
        public void Merge_IsIdempotent(List<LWW_SetElement<TestType>> existingAdds, List<LWW_SetElement<TestType>> existingRemoves, List<LWW_SetElement<TestType>> adds, List<LWW_SetElement<TestType>> removes)
        {
            _repository.PersistAdds(existingAdds);
            _repository.PersistRemoves(existingRemoves);

            _lwwSetService.Merge(adds, removes);
            _lwwSetService.Merge(adds, removes);
            _lwwSetService.Merge(adds, removes);

            var repositoryAdds = _repository.GetAdds();
            var repositoryRemoves = _repository.GetRemoves();

            AssertContains(adds, repositoryAdds);
            AssertContains(removes, repositoryRemoves);
        }

        //[Theory]
        //[AutoData]
        //public void Lookup_Added_ReturnsTrue(List<LWW_SetElement<TestType>> existingAdds, List<LWW_SetElement<TestType>> existingRemoves, TestType value, long timestamp)
        //{
        //    _repository.PersistAdds(existingAdds);
        //    _repository.PersistRemoves(existingRemoves);

        //    _lwwSetService.Add(value, timestamp);

        //    var lookup = _lwwSetService.Lookup(value);

        //    Assert.True(lookup);
        //}

        //[Theory]
        //[AutoData]
        //public void Lookup_Removed_ReturnsFalse(List<LWW_SetElement<TestType>> existingAdds, List<LWW_SetElement<TestType>> existingRemoves, TestType value, long timestamp)
        //{
        //    _repository.PersistAdds(existingAdds);
        //    _repository.PersistRemoves(existingRemoves);

        //    _lwwSetService.Add(value, timestamp);
        //    _lwwSetService.Remove(value, timestamp + 100);

        //    var lookup = _lwwSetService.Lookup(value);

        //    Assert.False(lookup);
        //}

        //[Theory]
        //[AutoData]
        //public void Lookup_ReAdded_ReturnsTrue(List<LWW_SetElement<TestType>> existingAdds, List<LWW_SetElement<TestType>> existingRemoves, TestType value, long timestamp)
        //{
        //    _repository.PersistAdds(existingAdds);
        //    _repository.PersistRemoves(existingRemoves);

        //    _lwwSetService.Add(value, timestamp);
        //    _lwwSetService.Remove(value, timestamp + 100);
        //    _lwwSetService.Add(value, timestamp + 200);

        //    var lookup = _lwwSetService.Lookup(value);

        //    Assert.True(lookup);
        //}

        private void AssertContains(List<LWW_SetElement<TestType>> expectedValues, IEnumerable<LWW_SetElement<TestType>> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}