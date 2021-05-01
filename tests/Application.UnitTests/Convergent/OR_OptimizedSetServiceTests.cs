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
    public class OR_OptimizedSetServiceTests
    {
        private readonly IOR_OptimizedSetRepository<TestType> _repository;
        private readonly OR_OptimizedSetService<TestType> _orSetService;

        public OR_OptimizedSetServiceTests()
        {
            _repository = new OR_OptimizedSetRepository();
            _orSetService = new OR_OptimizedSetService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value, Guid tag)
        {
            var element = new OR_OptimizedSetElement<TestType>(value, tag, false);
            _orSetService.Merge(new List<OR_OptimizedSetElement<TestType>> { element });

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(List<OR_OptimizedSetElement<TestType>> values)
        {
            _orSetService.Merge(values);

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SingleElement_AddsElementsToTheRepository(List<OR_OptimizedSetElement<TestType>> existingValues, TestType value, Guid tag)
        {
            _repository.PersistElements(existingValues);

            var element = new OR_OptimizedSetElement<TestType>(value, tag, false);
            _orSetService.Merge(new List<OR_OptimizedSetElement<TestType>> { element });

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElements_AddsElementsToTheRepository(List<OR_OptimizedSetElement<TestType>> existingValues, List<OR_OptimizedSetElement<TestType>> values)
        {
            _repository.PersistElements(existingValues);

            _orSetService.Merge(values);

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Merge_IsIdempotent(List<OR_OptimizedSetElement<TestType>> values)
        {
            _repository.PersistElements(values);

            _orSetService.Merge(values);
            _orSetService.Merge(values);
            _orSetService.Merge(values);

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsCommutative(Guid firstTag, Guid secondTag)
        {
            var firstElement = new OR_OptimizedSetElement<TestType>(Build(), firstTag, false);
            var secondElement = new OR_OptimizedSetElement<TestType>(Build(), secondTag, false);
            var thirdElement = new OR_OptimizedSetElement<TestType>(Build(), secondTag, false);
            var fourthElement = new OR_OptimizedSetElement<TestType>(Build(), firstTag, false);
            var fifthElement = new OR_OptimizedSetElement<TestType>(Build(), firstTag, false);

            var firstRepository = new OR_OptimizedSetRepository();
            var firstService = new OR_OptimizedSetService<TestType>(firstRepository);

            _repository.PersistElements(new List<OR_OptimizedSetElement<TestType>> { firstElement, secondElement, thirdElement });
            firstService.Merge(new List<OR_OptimizedSetElement<TestType>> { fourthElement, fifthElement });

            var firstRepositoryValues = firstRepository.GetElements();

            var secondRepository = new OR_OptimizedSetRepository();
            var secondService = new OR_OptimizedSetService<TestType>(secondRepository);

            _repository.PersistElements(new List<OR_OptimizedSetElement<TestType>> { fourthElement, fifthElement });
            secondService.Merge(new List<OR_OptimizedSetElement<TestType>> { firstElement, secondElement, thirdElement });

            var secondRepositoryValues = firstRepository.GetElements();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value, Guid tag)
        {
            _repository.PersistElements(new List<OR_OptimizedSetElement<TestType>> { new(value, tag, false) });
            _orSetService.Merge(new List<OR_OptimizedSetElement<TestType>> { new(value, tag, true) });

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x.Value, value) && x.Removed));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(List<OR_OptimizedSetElement<TestType>> values)
        {
            _repository.PersistElements(values);
            _orSetService.Merge(values);

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleElement_AddsElementsToTheRepository(List<OR_OptimizedSetElement<TestType>> existingValues, TestType value, Guid tag)
        {
            _repository.PersistElements(existingValues);
            _repository.PersistElements(new List<OR_OptimizedSetElement<TestType>> { new(value, tag, false) });

            _orSetService.Merge(new List<OR_OptimizedSetElement<TestType>> { new(value, tag, true) });

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x.Value, value) && x.Removed));
        }

        [Theory]
        [AutoData]
        public void Lookup_OptimizedSingleElementAdded_ReturnsTrue(TestType value, Guid tag)
        {
            _orSetService.Merge(new List<OR_OptimizedSetElement<TestType>> { new(value, tag, false) });

            var lookup = _orSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_OptimizedSeveralElementsWithDifferentTags_ReturnsTrue(TestType value, Guid firstTag, Guid secondTag)
        {
            var elements = new List<OR_OptimizedSetElement<TestType>>
            {
                new (value, firstTag, false),
                new (value, secondTag, false)
            };
            _orSetService.Merge(elements);

            var lookup = _orSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_NonExistingElement_ReturnsFalse(TestType value, Guid id)
        {
            _orSetService.Merge(new List<OR_OptimizedSetElement<TestType>> { new(value, id, true) });

            var lookup = _orSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AllTagsRemoved_ReturnsFalse(TestType value, Guid firstTag, Guid secondTag)
        {
            var elements = new List<OR_OptimizedSetElement<TestType>>
            {
                new(value, firstTag, true),
                new(value, secondTag, true)
            };

            _orSetService.Merge(elements);

            var lookup = _orSetService.Lookup(value);

            Assert.False(lookup);
        }

        private void AssertContains(List<OR_OptimizedSetElement<TestType>> expectedValues, IEnumerable<OR_OptimizedSetElement<TestType>> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}