using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Convergent.Set;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Application.UnitTests.Convergent
{
    public class OR_OptimizedSetServiceTests
    {
        private readonly IOR_OptimizedSetRepository<TestType> _repository;
        private readonly OR_OptimizedSetService<TestType> _orSetService;
        private readonly TestTypeBuilder _builder;

        public OR_OptimizedSetServiceTests()
        {
            _repository = new OR_OptimizedSetRepository();
            _orSetService = new OR_OptimizedSetService<TestType>(_repository);
            _builder = new TestTypeBuilder(new Random());
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value, Guid tag)
        {
            var element = new OR_OptimizedSetElement<TestType>(value, tag, false);
            _orSetService.Merge(new HashSet<OR_OptimizedSetElement<TestType>> { element }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(HashSet<OR_OptimizedSetElement<TestType>> values)
        {
            _orSetService.Merge(values.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SingleElement_AddsElementsToTheRepository(HashSet<OR_OptimizedSetElement<TestType>> existingValues, TestType value, Guid tag)
        {
            _repository.PersistElements(existingValues.ToImmutableHashSet());

            var element = new OR_OptimizedSetElement<TestType>(value, tag, false);
            _orSetService.Merge(new HashSet<OR_OptimizedSetElement<TestType>> { element }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElements_AddsElementsToTheRepository(HashSet<OR_OptimizedSetElement<TestType>> existingValues, HashSet<OR_OptimizedSetElement<TestType>> values)
        {
            _repository.PersistElements(existingValues.ToImmutableHashSet());

            _orSetService.Merge(values.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Merge_IsIdempotent(HashSet<OR_OptimizedSetElement<TestType>> values)
        {
            _repository.PersistElements(values.ToImmutableHashSet());

            _orSetService.Merge(values.ToImmutableHashSet());
            _orSetService.Merge(values.ToImmutableHashSet());
            _orSetService.Merge(values.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsCommutative(Guid firstTag, Guid secondTag)
        {
            var firstElement = new OR_OptimizedSetElement<TestType>(_builder.Build(), firstTag, false);
            var secondElement = new OR_OptimizedSetElement<TestType>(_builder.Build(), secondTag, false);
            var thirdElement = new OR_OptimizedSetElement<TestType>(_builder.Build(), secondTag, false);
            var fourthElement = new OR_OptimizedSetElement<TestType>(_builder.Build(), firstTag, false);
            var fifthElement = new OR_OptimizedSetElement<TestType>(_builder.Build(), firstTag, false);

            var firstRepository = new OR_OptimizedSetRepository();
            var firstService = new OR_OptimizedSetService<TestType>(firstRepository);

            _repository.PersistElements(new HashSet<OR_OptimizedSetElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet());
            firstService.Merge(new HashSet<OR_OptimizedSetElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet());

            var firstRepositoryValues = firstRepository.GetElements();

            var secondRepository = new OR_OptimizedSetRepository();
            var secondService = new OR_OptimizedSetService<TestType>(secondRepository);

            _repository.PersistElements(new HashSet<OR_OptimizedSetElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet());
            secondService.Merge(new HashSet<OR_OptimizedSetElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet());

            var secondRepositoryValues = firstRepository.GetElements();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value, Guid tag)
        {
            _repository.PersistElements(new HashSet<OR_OptimizedSetElement<TestType>> { new(value, tag, false) }.ToImmutableHashSet());
            _orSetService.Merge(new HashSet<OR_OptimizedSetElement<TestType>> { new(value, tag, true) }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x.Value, value) && x.Removed));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(HashSet<OR_OptimizedSetElement<TestType>> values)
        {
            _repository.PersistElements(values.ToImmutableHashSet());
            _orSetService.Merge(values.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleElement_AddsElementsToTheRepository(HashSet<OR_OptimizedSetElement<TestType>> existingValues, TestType value, Guid tag)
        {
            _repository.PersistElements(existingValues.ToImmutableHashSet());
            _repository.PersistElements(new HashSet<OR_OptimizedSetElement<TestType>> { new(value, tag, false) }.ToImmutableHashSet());

            _orSetService.Merge(new HashSet<OR_OptimizedSetElement<TestType>> { new(value, tag, true) }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x.Value, value) && x.Removed));
        }

        [Theory]
        [AutoData]
        public void Lookup_OptimizedSingleElementAdded_ReturnsTrue(TestType value, Guid tag)
        {
            _orSetService.Merge(new HashSet<OR_OptimizedSetElement<TestType>> { new(value, tag, false) }.ToImmutableHashSet());

            var lookup = _orSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_OptimizedSeveralElementsWithDifferentTags_ReturnsTrue(TestType value, Guid firstTag, Guid secondTag)
        {
            var elements = new HashSet<OR_OptimizedSetElement<TestType>>
            {
                new (value, firstTag, false),
                new (value, secondTag, false)
            };
            _orSetService.Merge(elements.ToImmutableHashSet());

            var lookup = _orSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_NonExistingElement_ReturnsFalse(TestType value, Guid id)
        {
            _orSetService.Merge(new HashSet<OR_OptimizedSetElement<TestType>> { new(value, id, true) }.ToImmutableHashSet());

            var lookup = _orSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AllTagsRemoved_ReturnsFalse(TestType value, Guid firstTag, Guid secondTag)
        {
            var elements = new HashSet<OR_OptimizedSetElement<TestType>>
            {
                new(value, firstTag, true),
                new(value, secondTag, true)
            };

            _orSetService.Merge(elements.ToImmutableHashSet());

            var lookup = _orSetService.Lookup(value);

            Assert.False(lookup);
        }

        private void AssertContains(HashSet<OR_OptimizedSetElement<TestType>> expectedValues, IEnumerable<OR_OptimizedSetElement<TestType>> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}