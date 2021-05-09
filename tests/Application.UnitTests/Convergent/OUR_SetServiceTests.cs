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
    public class OUR_SetServiceTests
    {
        private readonly IOUR_SetRepository<TestType> _repository;
        private readonly OUR_SetService<TestType> _orSetService;
        private readonly TestTypeBuilder _builder;

        public OUR_SetServiceTests()
        {
            _repository = new OUR_SetRepository();
            _orSetService = new OUR_SetService<TestType>(_repository);
            _builder = new TestTypeBuilder(new Random());
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value, Guid tag, long timestamp)
        {
            var element = new OUR_SetElement<TestType>(value, tag, timestamp);
            _orSetService.Merge(new List<OUR_SetElement<TestType>> { element }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(List<OUR_SetElement<TestType>> values)
        {
            _orSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_UpdatedElementWithHigherTimestamp_ReplacesElementsInRepository(TestType value, Guid tag, long timestamp)
        {
            var element = new OUR_SetElement<TestType>(value, tag, timestamp);

            _repository.PersistAdds(new List<OUR_SetElement<TestType>> { element }.ToImmutableHashSet());

            var newElement = new OUR_SetElement<TestType>(_builder.Build(value.Id), tag, timestamp + 1);
            _orSetService.LocalAdd(newElement.Value, newElement.Tag, newElement.Timestamp);

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(0, repositoryValues.Count(x => Equals(x, element)));
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, newElement)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_UpdatedElementWithLowerTimestamp_DoesNotDoAnything(TestType value, Guid tag, long timestamp)
        {
            var element = new OUR_SetElement<TestType>(value, tag, timestamp);

            _repository.PersistAdds(new List<OUR_SetElement<TestType>> { element }.ToImmutableHashSet());

            var newElement = new OUR_SetElement<TestType>(_builder.Build(value.Id), tag, timestamp - 1);

            _orSetService.LocalAdd(_builder.Build(value.Id), tag, timestamp - 1);

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
            Assert.Equal(0, repositoryValues.Count(x => Equals(x, newElement)));
        }
        [Theory]
        [AutoData]
        public void MergeAdds_SingleElement_AddsElementsToTheRepository(List<OUR_SetElement<TestType>> existingValues, TestType value, Guid tag, long timestamp)
        {
            _repository.PersistAdds(existingValues.ToImmutableHashSet());

            var element = new OUR_SetElement<TestType>(value, tag, timestamp);
            _orSetService.Merge(new List<OUR_SetElement<TestType>> { element }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElements_AddsElementsToTheRepository(List<OUR_SetElement<TestType>> existingValues, List<OUR_SetElement<TestType>> values)
        {
            _repository.PersistAdds(existingValues.ToImmutableHashSet());

            _orSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsIdempotent(List<OUR_SetElement<TestType>> values, TestType value, Guid tag, long timestamp)
        {
            _repository.PersistAdds(values.ToImmutableHashSet());

            var element = new OUR_SetElement<TestType>(value, tag, timestamp);

            values.Add(element);

            _orSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);
            _orSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);
            _orSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsCommutative(Guid firstTag, Guid secondTag, long timestamp)
        {
            var firstElement = new OUR_SetElement<TestType>(_builder.Build(), firstTag, timestamp);
            var secondElement = new OUR_SetElement<TestType>(_builder.Build(), secondTag, timestamp);
            var thirdElement = new OUR_SetElement<TestType>(_builder.Build(), secondTag, timestamp);
            var fourthElement = new OUR_SetElement<TestType>(_builder.Build(), firstTag, timestamp);
            var fifthElement = new OUR_SetElement<TestType>(_builder.Build(), firstTag, timestamp);

            var firstRepository = new OUR_SetRepository();
            var firstService = new OUR_SetService<TestType>(firstRepository);

            _repository.PersistAdds(new List<OUR_SetElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet());
            firstService.Merge(new List<OUR_SetElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);

            var firstRepositoryValues = firstRepository.GetAdds();

            var secondRepository = new OUR_SetRepository();
            var secondService = new OUR_SetService<TestType>(secondRepository);

            _repository.PersistAdds(new List<OUR_SetElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet());
            secondService.Merge(new List<OUR_SetElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);

            var secondRepositoryValues = firstRepository.GetAdds();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleValueWithEmptyRepository_AddsElementsToTheRepository(OUR_SetElement<TestType> value)
        {
            _repository.PersistAdds(new List<OUR_SetElement<TestType>> { value }.ToImmutableHashSet());
            _orSetService.Merge(ImmutableHashSet<OUR_SetElement<TestType>>.Empty, new List<OUR_SetElement<TestType>> { value }.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(List<OUR_SetElement<TestType>> values)
        {
            _repository.PersistAdds(values.ToImmutableHashSet());
            _orSetService.Merge(ImmutableHashSet<OUR_SetElement<TestType>>.Empty, values.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleElement_AddsElementsToTheRepository(List<OUR_SetElement<TestType>> existingValues, OUR_SetElement<TestType> value)
        {
            _repository.PersistRemoves(existingValues.ToImmutableHashSet());
            _repository.PersistAdds(new List<OUR_SetElement<TestType>> { value }.ToImmutableHashSet());

            _orSetService.Merge(ImmutableHashSet<OUR_SetElement<TestType>>.Empty, new List<OUR_SetElement<TestType>> { value }.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SeveralElements_AddsElementsToTheRepository(List<OUR_SetElement<TestType>> existingValues, List<OUR_SetElement<TestType>> values)
        {
            _repository.PersistRemoves(existingValues.ToImmutableHashSet());
            _repository.PersistAdds(values.ToImmutableHashSet());

            _orSetService.Merge(ImmutableHashSet<OUR_SetElement<TestType>>.Empty, values.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_IsIdempotent(List<OUR_SetElement<TestType>> values, OUR_SetElement<TestType> value)
        {
            _repository.PersistRemoves(values.ToImmutableHashSet());
            _repository.PersistAdds(new List<OUR_SetElement<TestType>> { value }.ToImmutableHashSet());

            values.Add(value);

            _orSetService.Merge(ImmutableHashSet<OUR_SetElement<TestType>>.Empty, values.ToImmutableHashSet());
            _orSetService.Merge(ImmutableHashSet<OUR_SetElement<TestType>>.Empty, values.ToImmutableHashSet());
            _orSetService.Merge(ImmutableHashSet<OUR_SetElement<TestType>>.Empty, values.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_IsCommutative(Guid firstTag, Guid secondTag, long timestamp)
        {
            var firstElement = new OUR_SetElement<TestType>(_builder.Build(), firstTag, timestamp);
            var secondElement = new OUR_SetElement<TestType>(_builder.Build(), secondTag, timestamp);
            var thirdElement = new OUR_SetElement<TestType>(_builder.Build(), secondTag, timestamp);
            var fourthElement = new OUR_SetElement<TestType>(_builder.Build(), firstTag, timestamp);
            var fifthElement = new OUR_SetElement<TestType>(_builder.Build(), firstTag, timestamp);

            var firstRepository = new OUR_SetRepository();
            var firstService = new OUR_SetService<TestType>(firstRepository);

            firstRepository.PersistRemoves(new List<OUR_SetElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet());
            firstRepository.PersistAdds(new List<OUR_SetElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet());
            firstService.Merge(ImmutableHashSet<OUR_SetElement<TestType>>.Empty, new List<OUR_SetElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet());

            var firstRepositoryValues = firstRepository.GetRemoves();

            var secondRepository = new OUR_SetRepository();
            var secondService = new OUR_SetService<TestType>(secondRepository);

            secondRepository.PersistRemoves(new List<OUR_SetElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet());
            secondRepository.PersistAdds(new List<OUR_SetElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet());
            secondService.Merge(ImmutableHashSet<OUR_SetElement<TestType>>.Empty, new List<OUR_SetElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet());

            var secondRepositoryValues = firstRepository.GetRemoves();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void Lookup_SingleElementAdded_ReturnsTrue(OUR_SetElement<TestType> value)
        {
            _orSetService.Merge(new List<OUR_SetElement<TestType>> { value }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);

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
            _orSetService.Merge(elements.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);

            var lookup = _orSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_NonExistingElement_ReturnsFalse(OUR_SetElement<TestType> value)
        {
            _orSetService.Merge(new List<OUR_SetElement<TestType>> { value }.ToImmutableHashSet(), new List<OUR_SetElement<TestType>> { value }.ToImmutableHashSet());

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
            _orSetService.Merge(elements.ToImmutableHashSet(), elements.ToImmutableHashSet());

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