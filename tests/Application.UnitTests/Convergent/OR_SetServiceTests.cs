using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Convergent;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;
using static CRDT.UnitTestHelpers.TestTypes.TestTypeBuilder;

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
        public void MergeAdds_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value, Guid tag)
        {
            var element = new OR_SetElement<TestType>(value, tag);
            _orSetService.Merge(new List<OR_SetElement<TestType>> { element }, new List<OR_SetElement<TestType>>());

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(List<OR_SetElement<TestType>> values)
        {
            _orSetService.Merge(values, new List<OR_SetElement<TestType>>());

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SingleElement_AddsElementsToTheRepository(List<OR_SetElement<TestType>> existingValues, TestType value, Guid tag)
        {
            _repository.PersistAdds(existingValues);

            var element = new OR_SetElement<TestType>(value, tag);
            _orSetService.Merge(new List<OR_SetElement<TestType>> { element }, new List<OR_SetElement<TestType>>());

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElements_AddsElementsToTheRepository(List<OR_SetElement<TestType>> existingValues, List<OR_SetElement<TestType>> values)
        {
            _repository.PersistAdds(existingValues);

            _orSetService.Merge(values, new List<OR_SetElement<TestType>>());

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsIdempotent(List<OR_SetElement<TestType>> values, TestType value, Guid tag)
        {
            _repository.PersistAdds(values);

            var element = new OR_SetElement<TestType>(value, tag);

            values.Add(element);

            _orSetService.Merge(values, new List<OR_SetElement<TestType>>());
            _orSetService.Merge(values, new List<OR_SetElement<TestType>>());
            _orSetService.Merge(values, new List<OR_SetElement<TestType>>());

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsCommutative(Guid firstTag, Guid secondTag)
        {
            var firstElement = new OR_SetElement<TestType>(Build(), firstTag);
            var secondElement = new OR_SetElement<TestType>(Build(), secondTag);
            var thirdElement = new OR_SetElement<TestType>(Build(), secondTag);
            var fourthElement = new OR_SetElement<TestType>(Build(), firstTag);
            var fifthElement = new OR_SetElement<TestType>(Build(), firstTag);

            var firstRepository = new OR_SetRepository();
            var firstService = new OR_SetService<TestType>(firstRepository);

            _repository.PersistAdds(new List<OR_SetElement<TestType>> { firstElement, secondElement, thirdElement });
            firstService.Merge(new List<OR_SetElement<TestType>> { fourthElement, fifthElement }, new List<OR_SetElement<TestType>>());

            var firstRepositoryValues = firstRepository.GetAdds();

            var secondRepository = new OR_SetRepository();
            var secondService = new OR_SetService<TestType>(secondRepository);

            _repository.PersistAdds(new List<OR_SetElement<TestType>> { fourthElement, fifthElement });
            secondService.Merge(new List<OR_SetElement<TestType>> { firstElement, secondElement, thirdElement }, new List<OR_SetElement<TestType>>());

            var secondRepositoryValues = firstRepository.GetAdds();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_WithoutExistingAdds_DoesNotAddToRepository(OR_SetElement<TestType> value)
        {
            _orSetService.Merge(new List<OR_SetElement<TestType>>(), new List<OR_SetElement<TestType>> { value });

            var repositoryValues = _repository.GetRemoves();
            Assert.Equal(0, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleValueWithEmptyRepository_AddsElementsToTheRepository(OR_SetElement<TestType> value)
        {
            _repository.PersistAdds(new List<OR_SetElement<TestType>> { value });
            _orSetService.Merge(new List<OR_SetElement<TestType>>(), new List<OR_SetElement<TestType>> { value });

            var repositoryValues = _repository.GetRemoves();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(List<OR_SetElement<TestType>> values)
        {
            _repository.PersistAdds(values);
            _orSetService.Merge(new List<OR_SetElement<TestType>>(), values);

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleElement_AddsElementsToTheRepository(List<OR_SetElement<TestType>> existingValues, OR_SetElement<TestType> value)
        {
            _repository.PersistRemoves(existingValues);
            _repository.PersistAdds(new List<OR_SetElement<TestType>> { value });

            _orSetService.Merge(new List<OR_SetElement<TestType>>(), new List<OR_SetElement<TestType>> { value });

            var repositoryValues = _repository.GetRemoves();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SeveralElements_AddsElementsToTheRepository(List<OR_SetElement<TestType>> existingValues, List<OR_SetElement<TestType>> values)
        {
            _repository.PersistRemoves(existingValues);
            _repository.PersistAdds(values);

            _orSetService.Merge(new List<OR_SetElement<TestType>>(), values);

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_IsIdempotent(List<OR_SetElement<TestType>> values, OR_SetElement<TestType> value)
        {
            _repository.PersistRemoves(values);
            _repository.PersistAdds(new List<OR_SetElement<TestType>> { value });

            values.Add(value);

            _orSetService.Merge(new List<OR_SetElement<TestType>>(), values);
            _orSetService.Merge(new List<OR_SetElement<TestType>>(), values);
            _orSetService.Merge(new List<OR_SetElement<TestType>>(), values);

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_IsCommutative(Guid firstTag, Guid secondTag)
        {
            var firstElement = new OR_SetElement<TestType>(Build(), firstTag);
            var secondElement = new OR_SetElement<TestType>(Build(), secondTag);
            var thirdElement = new OR_SetElement<TestType>(Build(), secondTag);
            var fourthElement = new OR_SetElement<TestType>(Build(), firstTag);
            var fifthElement = new OR_SetElement<TestType>(Build(), firstTag);

            var firstRepository = new OR_SetRepository();
            var firstService = new OR_SetService<TestType>(firstRepository);

            firstRepository.PersistRemoves(new List<OR_SetElement<TestType>> { firstElement, secondElement, thirdElement });
            firstRepository.PersistAdds(new List<OR_SetElement<TestType>> { fourthElement, fifthElement });
            firstService.Merge(new List<OR_SetElement<TestType>>(), new List<OR_SetElement<TestType>> { fourthElement, fifthElement });

            var firstRepositoryValues = firstRepository.GetRemoves();

            var secondRepository = new OR_SetRepository();
            var secondService = new OR_SetService<TestType>(secondRepository);

            secondRepository.PersistRemoves(new List<OR_SetElement<TestType>> { fourthElement, fifthElement });
            secondRepository.PersistAdds(new List<OR_SetElement<TestType>> { firstElement, secondElement, thirdElement });
            secondService.Merge(new List<OR_SetElement<TestType>>(), new List<OR_SetElement<TestType>> { firstElement, secondElement, thirdElement });

            var secondRepositoryValues = firstRepository.GetRemoves();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAddsAndRemoves_OnlyMergesRemovesWithAdds(OR_SetElement<TestType> firstValue, OR_SetElement<TestType> secondValue, OR_SetElement<TestType> thirdValue)
        {
            _orSetService.Merge(new List<OR_SetElement<TestType>> { firstValue, thirdValue }, new List<OR_SetElement<TestType>> { secondValue, thirdValue });

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
        public void Lookup_SingleElementAdded_ReturnsTrue(OR_SetElement<TestType> value)
        {
            _orSetService.Merge(new List<OR_SetElement<TestType>>{value}, new List<OR_SetElement<TestType>>());

            var lookup = _orSetService.Lookup(value.Value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SeveralElementsWithDifferentTags_ReturnsTrue(TestType value, Guid firstTag, Guid secondTag)
        {
            var elements = new List<OR_SetElement<TestType>>
            {
                new (value, firstTag),
                new (value, secondTag)
            };
            _orSetService.Merge(elements, new List<OR_SetElement<TestType>>());

            var lookup = _orSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_NonExistingElement_ReturnsFalse(OR_SetElement<TestType> value)
        {
            _orSetService.Merge(new List<OR_SetElement<TestType>> { value }, new List<OR_SetElement<TestType>>{value});

            var lookup = _orSetService.Lookup(value.Value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AllTagsRemoved_ReturnsFalse(TestType value, Guid firstTag, Guid secondTag)
        {
            var elements = new List<OR_SetElement<TestType>>
            {
                new (value, firstTag),
                new (value, secondTag)
            };
            _orSetService.Merge(elements, elements);

            var lookup = _orSetService.Lookup(value);

            Assert.False(lookup);
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