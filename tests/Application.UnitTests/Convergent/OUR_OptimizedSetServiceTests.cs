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
    public class OUR_OptimizedSetServiceTests
    {
        private readonly IOUR_OptimizedSetRepository<TestType> _repository;
        private readonly OUR_OptimizedSetService<TestType> _ourSetService;
        private readonly TestTypeBuilder _builder;

        public OUR_OptimizedSetServiceTests()
        {
            _repository = new OUR_OptimizedSetRepository();
            _ourSetService = new OUR_OptimizedSetService<TestType>(_repository);
            _builder = new TestTypeBuilder(new Random());
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value, Guid tag, long timestamp)
        {
            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, false);
            _ourSetService.Merge(new HashSet<OUR_OptimizedSetElement<TestType>> { element }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(HashSet<OUR_OptimizedSetElement<TestType>> values)
        {
            _ourSetService.Merge(values.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_UpdatedElementWithHigherTimestamp_ReplacesElementsInRepository(TestType value, Guid tag, long timestamp)
        {
            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, false);

            _repository.PersistElements(new HashSet<OUR_OptimizedSetElement<TestType>> { element }.ToImmutableHashSet());

            var newElement = new OUR_OptimizedSetElement<TestType>(_builder.Build(value.Id), tag, timestamp + 1, false);

            _ourSetService.LocalAdd(newElement.Value, newElement.Tag, newElement.Timestamp);

            var repositoryValues = _repository.GetElements();
            Assert.Equal(0, repositoryValues.Count(x => Equals(x, element)));
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, newElement)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_UpdatedElementWithLowerTimestamp_DoesNotDoAnything(TestType value, Guid tag, long timestamp)
        {
            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, false);

            _repository.PersistElements(new HashSet<OUR_OptimizedSetElement<TestType>> { element }.ToImmutableHashSet());

            var newElement = new OUR_OptimizedSetElement<TestType>(_builder.Build(value.Id), tag, timestamp - 1, false);

            _ourSetService.LocalAdd(newElement.Value, newElement.Tag, newElement.Timestamp);

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
            Assert.Equal(0, repositoryValues.Count(x => Equals(x, newElement)));
        }
        [Theory]
        [AutoData]
        public void MergeAdds_SingleElement_AddsElementsToTheRepository(HashSet<OUR_OptimizedSetElement<TestType>> existingValues, TestType value, Guid tag, long timestamp)
        {
            _repository.PersistElements(existingValues.ToImmutableHashSet());

            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, false);
            _ourSetService.Merge(new HashSet<OUR_OptimizedSetElement<TestType>> { element }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElements_AddsElementsToTheRepository(HashSet<OUR_OptimizedSetElement<TestType>> existingValues, HashSet<OUR_OptimizedSetElement<TestType>> values)
        {
            _repository.PersistElements(existingValues.ToImmutableHashSet());

            _ourSetService.Merge(values.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsIdempotent(HashSet<OUR_OptimizedSetElement<TestType>> values, TestType value, Guid tag, long timestamp)
        {
            _repository.PersistElements(values.ToImmutableHashSet());

            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, false);

            values.Add(element);

            _ourSetService.Merge(values.ToImmutableHashSet());
            _ourSetService.Merge(values.ToImmutableHashSet());
            _ourSetService.Merge(values.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsCommutative(Guid firstTag, Guid secondTag, long timestamp)
        {
            var firstElement = new OUR_OptimizedSetElement<TestType>(_builder.Build(), firstTag, timestamp, false);
            var secondElement = new OUR_OptimizedSetElement<TestType>(_builder.Build(), secondTag, timestamp, false);
            var thirdElement = new OUR_OptimizedSetElement<TestType>(_builder.Build(), secondTag, timestamp, false);
            var fourthElement = new OUR_OptimizedSetElement<TestType>(_builder.Build(), firstTag, timestamp, false);
            var fifthElement = new OUR_OptimizedSetElement<TestType>(_builder.Build(), firstTag, timestamp, false);

            var firstRepository = new OUR_OptimizedSetRepository();
            var firstService = new OUR_OptimizedSetService<TestType>(firstRepository);

            _repository.PersistElements(new HashSet<OUR_OptimizedSetElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet());
            firstService.Merge(new HashSet<OUR_OptimizedSetElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet());

            var firstRepositoryValues = firstRepository.GetElements();

            var secondRepository = new OUR_OptimizedSetRepository();
            var secondService = new OUR_OptimizedSetService<TestType>(secondRepository);

            _repository.PersistElements(new HashSet<OUR_OptimizedSetElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet());
            secondService.Merge(new HashSet<OUR_OptimizedSetElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet());

            var secondRepositoryValues = firstRepository.GetElements();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value, Guid tag, long timestamp)
        {
            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp + 1, true);

            _repository.PersistElements(new HashSet<OUR_OptimizedSetElement<TestType>> { new (value, tag, timestamp, false) }.ToImmutableHashSet());
            _ourSetService.Merge(new HashSet<OUR_OptimizedSetElement<TestType>> { element }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SeveralElements_AddsElementsToTheRepository(HashSet<OUR_OptimizedSetElement<TestType>> existingValues, TestType one, TestType two, Guid tag, long timestamp)
        {
            _repository.PersistElements(existingValues.ToImmutableHashSet());

            _ourSetService.Merge(new HashSet<OUR_OptimizedSetElement<TestType>>
            {
                new (one, tag, timestamp, true),
                new (two, tag, timestamp, true),
            }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, new OUR_OptimizedSetElement<TestType>(one, tag, timestamp, true))));
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, new OUR_OptimizedSetElement<TestType>(two, tag, timestamp, true))));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_IsIdempotent(TestType value, Guid tag, long timestamp)
        {
            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, true);

            _ourSetService.Merge(new HashSet<OUR_OptimizedSetElement<TestType>> { element }.ToImmutableHashSet());
            _ourSetService.Merge(new HashSet<OUR_OptimizedSetElement<TestType>> { element }.ToImmutableHashSet());
            _ourSetService.Merge(new HashSet<OUR_OptimizedSetElement<TestType>> { element }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void Lookup_SingleElementAdded_ReturnsTrue(TestType value, Guid tag, long timestamp)
        {
            _ourSetService.Merge(new HashSet<OUR_OptimizedSetElement<TestType>> { new (value, tag, timestamp, false) }.ToImmutableHashSet());

            var lookup = _ourSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SeveralElementsWithDifferentTags_ReturnsTrue(TestType value, Guid firstTag, Guid secondTag, long timestamp)
        {
            _ourSetService.Merge(new HashSet<OUR_OptimizedSetElement<TestType>>
            {
                new(value, firstTag, timestamp, false),
                new(value, secondTag, timestamp, false)
            }.ToImmutableHashSet());

            var lookup = _ourSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_NonExistingElement_ReturnsFalse(TestType value, Guid tag, long timestamp)
        {
            _ourSetService.Merge(new HashSet<OUR_OptimizedSetElement<TestType>>
            {
                new(value, tag, timestamp, true)
            }.ToImmutableHashSet());

            var lookup = _ourSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AllTagsRemoved_ReturnsFalse(TestType value, Guid firstTag, Guid secondTag, long timestamp)
        {
            _ourSetService.Merge(new HashSet<OUR_OptimizedSetElement<TestType>>
            {
                new(value, firstTag, timestamp, true),
                new(value, secondTag, timestamp, true)
            }.ToImmutableHashSet());

            var lookup = _ourSetService.Lookup(value);

            Assert.False(lookup);
        }

        private void AssertContains(HashSet<OUR_OptimizedSetElement<TestType>> expectedValues, IEnumerable<OUR_OptimizedSetElement<TestType>> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}