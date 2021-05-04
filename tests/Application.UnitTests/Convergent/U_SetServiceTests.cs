using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Convergent.Set;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;
using static CRDT.UnitTestHelpers.TestTypes.TestTypeBuilder;

namespace CRDT.Application.UnitTests.Convergent
{
    public class U_SetServiceTests
    {
        private readonly IU_SetRepository<TestType> _repository;
        private readonly U_SetService<TestType> _uSetService;

        public U_SetServiceTests()
        {
            _repository = new U_SetRepository();
            _uSetService = new U_SetService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void Merge_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value)
        {
            _uSetService.Merge(new List<U_SetElement<TestType>> { new(value, false) });

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Merge_SingleElement_AddsElementsToTheRepository(List<U_SetElement<TestType>> existingValues, TestType value)
        {
            _repository.PersistElements(existingValues);

            _uSetService.Merge(new List<U_SetElement<TestType>> { new(value, false) });

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Merge_IsIdempotent(List<U_SetElement<TestType>> values, U_SetElement<TestType> value)
        {
            _repository.PersistElements(values);

            values.Add(value);

            _uSetService.Merge(values);
            _uSetService.Merge(values);
            _uSetService.Merge(values);

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Fact]
        public void Merge_IsCommutative()
        {
            var firstValue = new U_SetElement<TestType>(Build(), false);
            var secondValue = new U_SetElement<TestType>(Build(), true);
            var thirdValue = new U_SetElement<TestType>(Build(), false);
            var fourthValue = new U_SetElement<TestType>(Build(), true);
            var fifthValue = new U_SetElement<TestType>(Build(), false);

            var firstRepository = new U_SetRepository();
            var firstService = new U_SetService<TestType>(firstRepository);

            _repository.PersistElements(new List<U_SetElement<TestType>> { firstValue, secondValue, thirdValue });
            firstService.Merge(new List<U_SetElement<TestType>> { fourthValue, fifthValue });

            var firstRepositoryValues = firstRepository.GetElements();

            var secondRepository = new U_SetRepository();
            var secondService = new U_SetService<TestType>(secondRepository);

            _repository.PersistElements(new List<U_SetElement<TestType>> { fourthValue, fifthValue });
            secondService.Merge(new List<U_SetElement<TestType>> { firstValue, secondValue, thirdValue });

            var secondRepositoryValues = firstRepository.GetElements();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleValueWithEmptyRepository_UpdatesElementInRepository(TestType value)
        {
            _repository.PersistElements(new List<U_SetElement<TestType>> { new(value, false) });

            var removeElement = new U_SetElement<TestType>(value, true);

            _uSetService.Merge(new List<U_SetElement<TestType>> { removeElement });

            var repositoryValues = _repository.GetElements();

            Assert.Single(repositoryValues);
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, removeElement)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_IsIdempotent(TestType value)
        {
            _repository.PersistElements(new List<U_SetElement<TestType>> { new(value, false) });

            var removeElement = new U_SetElement<TestType>(value, true);

            _uSetService.Merge(new List<U_SetElement<TestType>> { removeElement });
            _uSetService.Merge(new List<U_SetElement<TestType>> { removeElement });
            _uSetService.Merge(new List<U_SetElement<TestType>> { removeElement });

            var repositoryValues = _repository.GetElements();

            Assert.Single(repositoryValues);
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, removeElement)));
        }


        [Theory]
        [AutoData]
        public void Lookup_ReturnsTrue(TestType value)
        {
            var element = new U_SetElement<TestType>(value, false);

            _uSetService.Merge(new List<U_SetElement<TestType>> { element });

            var lookup = _uSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsFalse(TestType value)
        {
            _uSetService.Merge(new List<U_SetElement<TestType>> { new(value, false) });
            _uSetService.Merge(new List<U_SetElement<TestType>> { new(value, true) });

            var lookup = _uSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdd_ReturnsFalse(TestType value)
        {
            _uSetService.Merge(new List<U_SetElement<TestType>> { new(value, false) });
            _uSetService.Merge(new List<U_SetElement<TestType>> { new(value, true) });
            _uSetService.Merge(new List<U_SetElement<TestType>> { new(value, false) });

            var lookup = _uSetService.Lookup(value);

            Assert.False(lookup);
        }

        private void AssertContains(List<U_SetElement<TestType>> expectedValues, IEnumerable<U_SetElement<TestType>> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}