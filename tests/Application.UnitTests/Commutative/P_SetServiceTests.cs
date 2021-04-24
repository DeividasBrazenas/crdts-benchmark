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
    public class P_SetServiceTests
    {
        private readonly IP_SetRepository<TestType> _repository;
        private readonly P_SetService<TestType> _gSetService;

        public P_SetServiceTests()
        {
            _repository = new P_SetRepository();
            _gSetService = new P_SetService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void Add_NoExistingValues_AddsElementToTheRepository(TestType value)
        {
            _gSetService.Add(value);

            var repositoryValues = _repository.GetAdds();
            Assert.Contains(value, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(List<TestType> adds, TestType value)
        {
            _repository.PersistAdds(adds);

            _gSetService.Add(value);

            var repositoryValues = _repository.GetAdds();
            Assert.Contains(value, repositoryValues);
        }     
        
        [Theory]
        [AutoData]
        public void Add_IsIdempotent(List<TestType> adds, TestType value)
        {
            _repository.PersistAdds(adds);

            _gSetService.Add(value);
            _gSetService.Add(value);
            _gSetService.Add(value);

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(1, repositoryValues.Count(v => Equals(v, value)));
        }

        [Theory]
        [AutoData]
        public void Remove_AddDoesNotExist_DoesNotAddElementToTheRepository(TestType value)
        {
            _gSetService.Remove(value);

            var repositoryValues = _repository.GetRemoves();
            Assert.DoesNotContain(value, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddExists_AddsElementToTheRepository(TestType value)
        {
            _gSetService.Add(value);
            _gSetService.Remove(value);

            var repositoryValues = _repository.GetRemoves();
            Assert.Contains(value, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Remove_IsIdempotent(TestType value)
        {
            _gSetService.Add(value);
            _gSetService.Remove(value);
            _gSetService.Remove(value);
            _gSetService.Remove(value);

            var repositoryValues = _repository.GetRemoves();
            Assert.Equal(1, repositoryValues.Count(v => Equals(v, value)));
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsTrue(List<TestType> existingAdds, List<TestType> existingRemoves, TestType value)
        {
            _repository.PersistAdds(existingAdds);
            _repository.PersistRemoves(existingRemoves);

            _gSetService.Add(value);

            var lookup = _gSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsFalse(List<TestType> existingAdds, List<TestType> existingRemoves, TestType value)
        {
            _repository.PersistAdds(existingAdds);
            _repository.PersistRemoves(existingRemoves);

            _gSetService.Add(value);
            _gSetService.Remove(value);

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