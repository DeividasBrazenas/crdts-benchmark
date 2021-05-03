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
    public class OUR_OptimizedSetServiceTests
    {
        private readonly IOUR_OptimizedSetRepository<TestType> _repository;
        private readonly OUR_OptimizedSetService<TestType> _ourSetService;

        public OUR_OptimizedSetServiceTests()
        {
            _repository = new OUR_OptimizedSetRepository();
            _ourSetService = new OUR_OptimizedSetService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value, Guid tag, long timestamp)
        {
            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, false);
            _ourSetService.Merge(new List<OUR_OptimizedSetElement<TestType>> { element });

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(List<OUR_OptimizedSetElement<TestType>> values)
        {
            _ourSetService.Merge(values);

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_UpdatedElementWithHigherTimestamp_ReplacesElementsInRepository(TestType value, Guid tag, long timestamp)
        {
            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, false);

            _repository.PersistElements(new List<OUR_OptimizedSetElement<TestType>> { element });

            var newElement = new OUR_OptimizedSetElement<TestType>(Build(value.Id), tag, timestamp + 1, false);

            _ourSetService.Merge(new List<OUR_OptimizedSetElement<TestType>> { newElement });

            var repositoryValues = _repository.GetElements();
            Assert.Equal(0, repositoryValues.Count(x => Equals(x, element)));
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, newElement)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_UpdatedElementWithLowerTimestamp_DoesNotDoAnything(TestType value, Guid tag, long timestamp)
        {
            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, false);

            _repository.PersistElements(new List<OUR_OptimizedSetElement<TestType>> { element });

            var newElement = new OUR_OptimizedSetElement<TestType>(Build(value.Id), tag, timestamp - 1, false);

            _ourSetService.Merge(new List<OUR_OptimizedSetElement<TestType>> { newElement });

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
            Assert.Equal(0, repositoryValues.Count(x => Equals(x, newElement)));
        }
        [Theory]
        [AutoData]
        public void MergeAdds_SingleElement_AddsElementsToTheRepository(List<OUR_OptimizedSetElement<TestType>> existingValues, TestType value, Guid tag, long timestamp)
        {
            _repository.PersistElements(existingValues);

            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, false);
            _ourSetService.Merge(new List<OUR_OptimizedSetElement<TestType>> { element });

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElements_AddsElementsToTheRepository(List<OUR_OptimizedSetElement<TestType>> existingValues, List<OUR_OptimizedSetElement<TestType>> values)
        {
            _repository.PersistElements(existingValues);

            _ourSetService.Merge(values);

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsIdempotent(List<OUR_OptimizedSetElement<TestType>> values, TestType value, Guid tag, long timestamp)
        {
            _repository.PersistElements(values);

            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, false);

            values.Add(element);

            _ourSetService.Merge(values);
            _ourSetService.Merge(values);
            _ourSetService.Merge(values);

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsCommutative(Guid firstTag, Guid secondTag, long timestamp)
        {
            var firstElement = new OUR_OptimizedSetElement<TestType>(Build(), firstTag, timestamp, false);
            var secondElement = new OUR_OptimizedSetElement<TestType>(Build(), secondTag, timestamp, false);
            var thirdElement = new OUR_OptimizedSetElement<TestType>(Build(), secondTag, timestamp, false);
            var fourthElement = new OUR_OptimizedSetElement<TestType>(Build(), firstTag, timestamp, false);
            var fifthElement = new OUR_OptimizedSetElement<TestType>(Build(), firstTag, timestamp, false);

            var firstRepository = new OUR_OptimizedSetRepository();
            var firstService = new OUR_OptimizedSetService<TestType>(firstRepository);

            _repository.PersistElements(new List<OUR_OptimizedSetElement<TestType>> { firstElement, secondElement, thirdElement });
            firstService.Merge(new List<OUR_OptimizedSetElement<TestType>> { fourthElement, fifthElement });

            var firstRepositoryValues = firstRepository.GetElements();

            var secondRepository = new OUR_OptimizedSetRepository();
            var secondService = new OUR_OptimizedSetService<TestType>(secondRepository);

            _repository.PersistElements(new List<OUR_OptimizedSetElement<TestType>> { fourthElement, fifthElement });
            secondService.Merge(new List<OUR_OptimizedSetElement<TestType>> { firstElement, secondElement, thirdElement });

            var secondRepositoryValues = firstRepository.GetElements();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value, Guid tag, long timestamp)
        {
            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp + 1, true);

            _repository.PersistElements(new List<OUR_OptimizedSetElement<TestType>> { new (value, tag, timestamp, false) });
            _ourSetService.Merge(new List<OUR_OptimizedSetElement<TestType>> { element });

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SeveralElements_AddsElementsToTheRepository(List<OUR_OptimizedSetElement<TestType>> existingValues, TestType one, TestType two, Guid tag, long timestamp)
        {
            _repository.PersistElements(existingValues);

            _ourSetService.Merge(new List<OUR_OptimizedSetElement<TestType>>
            {
                new (one, tag, timestamp, true),
                new (two, tag, timestamp, true),
            });

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, new OUR_OptimizedSetElement<TestType>(one, tag, timestamp, true))));
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, new OUR_OptimizedSetElement<TestType>(two, tag, timestamp, true))));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_IsIdempotent(TestType value, Guid tag, long timestamp)
        {
            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, true);

            _ourSetService.Merge(new List<OUR_OptimizedSetElement<TestType>> { element });
            _ourSetService.Merge(new List<OUR_OptimizedSetElement<TestType>> { element });
            _ourSetService.Merge(new List<OUR_OptimizedSetElement<TestType>> { element });

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void Lookup_SingleElementAdded_ReturnsTrue(TestType value, Guid tag, long timestamp)
        {
            _ourSetService.Merge(new List<OUR_OptimizedSetElement<TestType>> { new (value, tag, timestamp, false) });

            var lookup = _ourSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SeveralElementsWithDifferentTags_ReturnsTrue(TestType value, Guid firstTag, Guid secondTag, long timestamp)
        {
            _ourSetService.Merge(new List<OUR_OptimizedSetElement<TestType>>
            {
                new(value, firstTag, timestamp, false),
                new(value, secondTag, timestamp, false)
            });

            var lookup = _ourSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_NonExistingElement_ReturnsFalse(TestType value, Guid tag, long timestamp)
        {
            _ourSetService.Merge(new List<OUR_OptimizedSetElement<TestType>>
            {
                new(value, tag, timestamp, true)
            });

            var lookup = _ourSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AllTagsRemoved_ReturnsFalse(TestType value, Guid firstTag, Guid secondTag, long timestamp)
        {
            _ourSetService.Merge(new List<OUR_OptimizedSetElement<TestType>>
            {
                new(value, firstTag, timestamp, true),
                new(value, secondTag, timestamp, true)
            });

            var lookup = _ourSetService.Lookup(value);

            Assert.False(lookup);
        }

        private void AssertContains(List<OUR_OptimizedSetElement<TestType>> expectedValues, IEnumerable<OUR_OptimizedSetElement<TestType>> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}