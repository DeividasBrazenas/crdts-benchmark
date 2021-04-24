using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Commutative;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Application.UnitTests.Commutative
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

        [Theory]
        [AutoData]
        public void Add_NoExistingValues_AddsElementsToTheRepository(TestType value)
        {
            _gSetService.Add(value);

            var repositoryValues = _repository.GetValues();
            Assert.Equal(1, repositoryValues.Count(v => Equals(v, value)));
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementsToTheRepository(List<TestType> existingValues, TestType value)
        {
            _repository.PersistValues(existingValues);

            _gSetService.Add(value);

            var repositoryValues = _repository.GetValues();
            Assert.Equal(1, repositoryValues.Count(v => Equals(v, value)));
        }

        [Theory]
        [AutoData]
        public void Add_IsIdempotent(List<TestType> existingValues, TestType value)
        {
            _repository.PersistValues(existingValues);

            _gSetService.Add(value);
            _gSetService.Add(value);
            _gSetService.Add(value);

            var repositoryValues = _repository.GetValues();
            Assert.Equal(1, repositoryValues.Count(v => Equals(v, value)));
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsTrue(List<TestType> existingValues, TestType value)
        {
            _repository.PersistValues(existingValues);

            _gSetService.Add(value);

            var lookup = _gSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsFalse(List<TestType> existingValues, TestType value)
        {
            _repository.PersistValues(existingValues);

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