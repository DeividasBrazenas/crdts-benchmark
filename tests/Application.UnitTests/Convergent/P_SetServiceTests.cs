using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Convergent;
using CRDT.Application.Convergent.Set;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;
using static CRDT.UnitTestHelpers.TestTypes.TestTypeBuilder;

namespace CRDT.Application.UnitTests.Convergent
{
    public class P_SetServiceTests
    {
        private readonly IP_SetRepository<TestType> _repository;
        private readonly P_SetService<TestType> _pSetService;

        public P_SetServiceTests()
        {
            _repository = new P_SetRepository();
            _pSetService = new P_SetService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value)
        {
            _pSetService.Merge(new List<TestType> { value }, new List<TestType>());

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(List<TestType> values)
        {
            _pSetService.Merge(values, new List<TestType>());

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SingleElement_AddsElementsToTheRepository(List<TestType> existingValues, TestType value)
        {
            _repository.PersistAdds(existingValues);

            _pSetService.Merge(new List<TestType> { value }, new List<TestType>());

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElements_AddsElementsToTheRepository(List<TestType> existingValues, List<TestType> values)
        {
            _repository.PersistAdds(existingValues);

            _pSetService.Merge(values, new List<TestType>());

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsIdempotent(List<TestType> values, TestType value)
        {
            _repository.PersistAdds(values);

            values.Add(value);

            _pSetService.Merge(values, new List<TestType>());
            _pSetService.Merge(values, new List<TestType>());
            _pSetService.Merge(values, new List<TestType>());

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Fact]
        public void MergeAdds_IsCommutative()
        {
            var firstValue = Build();
            var secondValue = Build();
            var thirdValue = Build();
            var fourthValue = Build();
            var fifthValue = Build();

            var firstRepository = new P_SetRepository();
            var firstService = new P_SetService<TestType>(firstRepository);

            _repository.PersistAdds(new List<TestType> { firstValue, secondValue, thirdValue });
            firstService.Merge(new List<TestType> { fourthValue, fifthValue }, new List<TestType>());

            var firstRepositoryValues = firstRepository.GetAdds();

            var secondRepository = new P_SetRepository();
            var secondService = new P_SetService<TestType>(secondRepository);

            _repository.PersistAdds(new List<TestType> { fourthValue, fifthValue });
            secondService.Merge(new List<TestType> { firstValue, secondValue, thirdValue }, new List<TestType>());

            var secondRepositoryValues = firstRepository.GetAdds();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_WithoutExistingAdds_DoesNotAddToRepository(TestType value)
        {
            _pSetService.Merge(new List<TestType>(), new List<TestType> { value });

            var repositoryValues = _repository.GetRemoves();
            Assert.Equal(0, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value)
        {
            _repository.PersistAdds(new List<TestType> { value });
            _pSetService.Merge(new List<TestType>(), new List<TestType> { value });

            var repositoryValues = _repository.GetRemoves();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(List<TestType> values)
        {
            _repository.PersistAdds(values);
            _pSetService.Merge(new List<TestType>(), values);

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleElement_AddsElementsToTheRepository(List<TestType> existingValues, TestType value)
        {
            _repository.PersistRemoves(existingValues);
            _repository.PersistAdds(new List<TestType> { value });

            _pSetService.Merge(new List<TestType>(), new List<TestType> { value });

            var repositoryValues = _repository.GetRemoves();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SeveralElements_AddsElementsToTheRepository(List<TestType> existingValues, List<TestType> values)
        {
            _repository.PersistRemoves(existingValues);
            _repository.PersistAdds(values);

            _pSetService.Merge(new List<TestType>(), values);

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_IsIdempotent(List<TestType> values, TestType value)
        {
            _repository.PersistRemoves(values);
            _repository.PersistAdds(new List<TestType> { value });

            values.Add(value);

            _pSetService.Merge(new List<TestType>(), values);
            _pSetService.Merge(new List<TestType>(), values);
            _pSetService.Merge(new List<TestType>(), values);

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Fact]
        public void MergeRemoves_IsCommutative()
        {
            var firstValue = Build();
            var secondValue = Build();
            var thirdValue = Build();
            var fourthValue = Build();
            var fifthValue = Build();

            var firstRepository = new P_SetRepository();
            var firstService = new P_SetService<TestType>(firstRepository);

            firstRepository.PersistRemoves(new List<TestType> { firstValue, secondValue, thirdValue });
            firstRepository.PersistAdds(new List<TestType> { fourthValue, fifthValue });
            firstService.Merge(new List<TestType>(), new List<TestType> { fourthValue, fifthValue });

            var firstRepositoryValues = firstRepository.GetRemoves();

            var secondRepository = new P_SetRepository();
            var secondService = new P_SetService<TestType>(secondRepository);

            secondRepository.PersistRemoves(new List<TestType> { fourthValue, fifthValue });
            secondRepository.PersistAdds(new List<TestType> { firstValue, secondValue, thirdValue });
            secondService.Merge(new List<TestType>(), new List<TestType> { firstValue, secondValue, thirdValue });

            var secondRepositoryValues = firstRepository.GetRemoves();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAddsAndRemoves_OnlyMergesRemovesWithAdds(TestType firstValue, TestType secondValue, TestType thirdValue)
        {
            _pSetService.Merge(new List<TestType> { firstValue, thirdValue }, new List<TestType> { secondValue, thirdValue });

            var repositoryAdds = _repository.GetAdds();
            var repositoryRemoves = _repository.GetRemoves();

            Assert.Equal(1, repositoryAdds.Count(x => Equals(x, firstValue)));
            Assert.Equal(0, repositoryAdds.Count(x => Equals(x, secondValue)));
            Assert.Equal(1, repositoryAdds.Count(x => Equals(x, thirdValue)));
            Assert.Equal(0, repositoryRemoves.Count(x => Equals(x, firstValue)));
            Assert.Equal(0, repositoryRemoves.Count(x => Equals(x, secondValue)));
            Assert.Equal(1, repositoryRemoves.Count(x => Equals(x, thirdValue)));
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsTrue(TestType value)
        {
            _pSetService.Merge(new List<TestType> { value }, new List<TestType>());

            var lookup = _pSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsFalse(TestType value)
        {
            _pSetService.Merge(new List<TestType> { value }, new List<TestType> { value });

            var lookup = _pSetService.Lookup(value);

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