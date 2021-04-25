using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Convergent;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Application.UnitTests.Convergent
{
    public class G_SetServiceTests
    {
        private readonly IG_SetRepository<TestType> _repository;
        private readonly G_SetService<TestType> _gSetService;

        public G_SetServiceTests()
        {
            _repository = new G_SetRepository();
            _gSetService = new G_SetService<TestType>(_repository);
        }

        //[Theory]
        //[AutoData]
        //public void Add_NoExistingValues_AddsElementToTheRepository(TestType value)
        //{
        //    _gSetService.Add(value);

        //    var repositoryValues = _repository.GetValues();
        //    Assert.Contains(value, repositoryValues);
        //}

        //[Theory]
        //[AutoData]
        //public void Add_WithExistingValues_AddsElementToTheRepository(List<TestType> values, TestType value)
        //{
        //    _repository.PersistValues(values);

        //    _gSetService.Add(value);

        //    var repositoryValues = _repository.GetValues();
        //    Assert.Contains(value, repositoryValues);
        //}

        [Theory]
        [AutoData]
        public void Merge_NoExistingValues_AddsElementsToTheRepository(List<TestType> values)
        {
            _gSetService.Merge(values);

            var repositoryValues = _repository.GetValues();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Merge_WithExistingValues_AddsElementsToTheRepository(List<TestType> existingValues, List<TestType> values)
        {
            _repository.PersistValues(existingValues);

            _gSetService.Merge(values);

            var repositoryValues = _repository.GetValues();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Merge_IsIdempotent(List<TestType> values, TestType value)
        {
            _repository.PersistValues(values);

            values.Add(value);

            _gSetService.Merge(values);
            _gSetService.Merge(values);
            _gSetService.Merge(values);

            var repositoryValues = _repository.GetValues();
            AssertContains(values, repositoryValues);
        }

        //[Theory]
        //[AutoData]
        //public void Lookup_ReturnsTrue(List<TestType> values, TestType value)
        //{
        //    _repository.PersistValues(values);

        //    _gSetService.Add(value);

        //    var lookup = _gSetService.Lookup(value);

        //    Assert.True(lookup);
        //}

        [Theory]
        [AutoData]
        public void Lookup_ReturnsFalse(List<TestType> values, TestType value)
        {
            _repository.PersistValues(values);

            var lookup = _gSetService.Lookup(value);

            Assert.False(lookup);
        }

        private void AssertContains(List<TestType> expectedValues, IEnumerable<TestType> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}