using System;
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
    public class OUR_SetServiceTests
    {
        private readonly IOUR_SetRepository<TestType> _repository;
        private readonly OUR_SetService<TestType> _orSetService;

        public OUR_SetServiceTests()
        {
            _repository = new OUR_SetRepository();
            _orSetService = new OUR_SetService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value, Guid tag, long timestamp)
        {
            var element = new OUR_SetElement<TestType>(value, tag, timestamp);
            _orSetService.Merge(new List<OUR_SetElement<TestType>> { element }, new List<OUR_SetElement<TestType>>());

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(List<OUR_SetElement<TestType>> values)
        {
            _orSetService.Merge(values, new List<OUR_SetElement<TestType>>());

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_UpdatedElementWithHigherTimestamp_ReplacesElementsInRepository(TestType value, Guid tag, long timestamp)
        {
            var element = new OUR_SetElement<TestType>(value, tag, timestamp);

            _repository.PersistAdds(new List<OUR_SetElement<TestType>> { element });

            var newElement = new OUR_SetElement<TestType>(Build(value.Id), tag, timestamp + 1);

            _orSetService.Merge(new List<OUR_SetElement<TestType>> { newElement }, new List<OUR_SetElement<TestType>>());

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(0, repositoryValues.Count(x => Equals(x, element)));
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, newElement)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_UpdatedElementWithLowerTimestamp_DoesNotDoAnything(TestType value, Guid tag, long timestamp)
        {
            var element = new OUR_SetElement<TestType>(value, tag, timestamp);

            _repository.PersistAdds(new List<OUR_SetElement<TestType>> { element });

            var newElement = new OUR_SetElement<TestType>(Build(value.Id), tag, timestamp - 1);

            _orSetService.Merge(new List<OUR_SetElement<TestType>> { newElement }, new List<OUR_SetElement<TestType>>());

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
            Assert.Equal(0, repositoryValues.Count(x => Equals(x, newElement)));
        }
        [Theory]
        [AutoData]
        public void MergeAdds_SingleElement_AddsElementsToTheRepository(List<OUR_SetElement<TestType>> existingValues, TestType value, Guid tag, long timestamp)
        {
            _repository.PersistAdds(existingValues);

            var element = new OUR_SetElement<TestType>(value, tag, timestamp);
            _orSetService.Merge(new List<OUR_SetElement<TestType>> { element }, new List<OUR_SetElement<TestType>>());

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElements_AddsElementsToTheRepository(List<OUR_SetElement<TestType>> existingValues, List<OUR_SetElement<TestType>> values)
        {
            _repository.PersistAdds(existingValues);

            _orSetService.Merge(values, new List<OUR_SetElement<TestType>>());

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsIdempotent(List<OUR_SetElement<TestType>> values, TestType value, Guid tag, long timestamp)
        {
            _repository.PersistAdds(values);

            var element = new OUR_SetElement<TestType>(value, tag, timestamp);

            values.Add(element);

            _orSetService.Merge(values, new List<OUR_SetElement<TestType>>());
            _orSetService.Merge(values, new List<OUR_SetElement<TestType>>());
            _orSetService.Merge(values, new List<OUR_SetElement<TestType>>());

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsCommutative(Guid firstTag, Guid secondTag, long timestamp)
        {
            var firstElement = new OUR_SetElement<TestType>(Build(), firstTag, timestamp);
            var secondElement = new OUR_SetElement<TestType>(Build(), secondTag, timestamp);
            var thirdElement = new OUR_SetElement<TestType>(Build(), secondTag, timestamp);
            var fourthElement = new OUR_SetElement<TestType>(Build(), firstTag, timestamp);
            var fifthElement = new OUR_SetElement<TestType>(Build(), firstTag, timestamp);

            var firstRepository = new OUR_SetRepository();
            var firstService = new OUR_SetService<TestType>(firstRepository);

            _repository.PersistAdds(new List<OUR_SetElement<TestType>> { firstElement, secondElement, thirdElement });
            firstService.Merge(new List<OUR_SetElement<TestType>> { fourthElement, fifthElement }, new List<OUR_SetElement<TestType>>());

            var firstRepositoryValues = firstRepository.GetAdds();

            var secondRepository = new OUR_SetRepository();
            var secondService = new OUR_SetService<TestType>(secondRepository);

            _repository.PersistAdds(new List<OUR_SetElement<TestType>> { fourthElement, fifthElement });
            secondService.Merge(new List<OUR_SetElement<TestType>> { firstElement, secondElement, thirdElement }, new List<OUR_SetElement<TestType>>());

            var secondRepositoryValues = firstRepository.GetAdds();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_WithoutExistingAdds_DoesNotAddToRepository(OUR_SetElement<TestType> value)
        {
            _orSetService.Merge(new List<OUR_SetElement<TestType>>(), new List<OUR_SetElement<TestType>> { value });

            var repositoryValues = _repository.GetRemoves();
            Assert.Equal(0, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleValueWithEmptyRepository_AddsElementsToTheRepository(OUR_SetElement<TestType> value)
        {
            _repository.PersistAdds(new List<OUR_SetElement<TestType>> { value });
            _orSetService.Merge(new List<OUR_SetElement<TestType>>(), new List<OUR_SetElement<TestType>> { value });

            var repositoryValues = _repository.GetRemoves();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(List<OUR_SetElement<TestType>> values)
        {
            _repository.PersistAdds(values);
            _orSetService.Merge(new List<OUR_SetElement<TestType>>(), values);

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleElement_AddsElementsToTheRepository(List<OUR_SetElement<TestType>> existingValues, OUR_SetElement<TestType> value)
        {
            _repository.PersistRemoves(existingValues);
            _repository.PersistAdds(new List<OUR_SetElement<TestType>> { value });

            _orSetService.Merge(new List<OUR_SetElement<TestType>>(), new List<OUR_SetElement<TestType>> { value });

            var repositoryValues = _repository.GetRemoves();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SeveralElements_AddsElementsToTheRepository(List<OUR_SetElement<TestType>> existingValues, List<OUR_SetElement<TestType>> values)
        {
            _repository.PersistRemoves(existingValues);
            _repository.PersistAdds(values);

            _orSetService.Merge(new List<OUR_SetElement<TestType>>(), values);

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_IsIdempotent(List<OUR_SetElement<TestType>> values, OUR_SetElement<TestType> value)
        {
            _repository.PersistRemoves(values);
            _repository.PersistAdds(new List<OUR_SetElement<TestType>> { value });

            values.Add(value);

            _orSetService.Merge(new List<OUR_SetElement<TestType>>(), values);
            _orSetService.Merge(new List<OUR_SetElement<TestType>>(), values);
            _orSetService.Merge(new List<OUR_SetElement<TestType>>(), values);

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_IsCommutative(Guid firstTag, Guid secondTag, long timestamp)
        {
            var firstElement = new OUR_SetElement<TestType>(Build(), firstTag, timestamp);
            var secondElement = new OUR_SetElement<TestType>(Build(), secondTag, timestamp);
            var thirdElement = new OUR_SetElement<TestType>(Build(), secondTag, timestamp);
            var fourthElement = new OUR_SetElement<TestType>(Build(), firstTag, timestamp);
            var fifthElement = new OUR_SetElement<TestType>(Build(), firstTag, timestamp);

            var firstRepository = new OUR_SetRepository();
            var firstService = new OUR_SetService<TestType>(firstRepository);

            firstRepository.PersistRemoves(new List<OUR_SetElement<TestType>> { firstElement, secondElement, thirdElement });
            firstRepository.PersistAdds(new List<OUR_SetElement<TestType>> { fourthElement, fifthElement });
            firstService.Merge(new List<OUR_SetElement<TestType>>(), new List<OUR_SetElement<TestType>> { fourthElement, fifthElement });

            var firstRepositoryValues = firstRepository.GetRemoves();

            var secondRepository = new OUR_SetRepository();
            var secondService = new OUR_SetService<TestType>(secondRepository);

            secondRepository.PersistRemoves(new List<OUR_SetElement<TestType>> { fourthElement, fifthElement });
            secondRepository.PersistAdds(new List<OUR_SetElement<TestType>> { firstElement, secondElement, thirdElement });
            secondService.Merge(new List<OUR_SetElement<TestType>>(), new List<OUR_SetElement<TestType>> { firstElement, secondElement, thirdElement });

            var secondRepositoryValues = firstRepository.GetRemoves();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAddsAndRemoves_OnlyMergesRemovesWithAdds(OUR_SetElement<TestType> firstValue, OUR_SetElement<TestType> secondValue, OUR_SetElement<TestType> thirdValue)
        {
            _orSetService.Merge(new List<OUR_SetElement<TestType>> { firstValue, thirdValue }, new List<OUR_SetElement<TestType>> { secondValue, thirdValue });

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
        public void Lookup_SingleElementAdded_ReturnsTrue(OUR_SetElement<TestType> value)
        {
            _orSetService.Merge(new List<OUR_SetElement<TestType>> { value }, new List<OUR_SetElement<TestType>>());

            var lookup = _orSetService.Lookup(value.Value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SeveralElementsWithDifferentTags_ReturnsTrue(TestType value, Guid firstTag, Guid secondTag, long timestamp)
        {
            var elements = new List<OUR_SetElement<TestType>>
            {
                new (value, firstTag, timestamp),
                new (value, secondTag, timestamp)
            };
            _orSetService.Merge(elements, new List<OUR_SetElement<TestType>>());

            var lookup = _orSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_NonExistingElement_ReturnsFalse(OUR_SetElement<TestType> value)
        {
            _orSetService.Merge(new List<OUR_SetElement<TestType>> { value }, new List<OUR_SetElement<TestType>> { value });

            var lookup = _orSetService.Lookup(value.Value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AllTagsRemoved_ReturnsFalse(TestType value, Guid firstTag, Guid secondTag, long timestamp)
        {
            var elements = new List<OUR_SetElement<TestType>>
            {
                new (value, firstTag, timestamp),
                new (value, secondTag, timestamp)
            };
            _orSetService.Merge(elements, elements);

            var lookup = _orSetService.Lookup(value);

            Assert.False(lookup);
        }

        private void AssertContains(List<OUR_SetElement<TestType>> expectedValues, IEnumerable<OUR_SetElement<TestType>> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}