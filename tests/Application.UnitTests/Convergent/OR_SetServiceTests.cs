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
    public class OR_SetServiceTests
    {
        private readonly IOR_SetRepository<TestType> _repository;
        private readonly OR_SetService<TestType> _orSetService;
        private readonly TestTypeBuilder _builder;
        public OR_SetServiceTests()
        {
            _repository = new OR_SetRepository();
            _orSetService = new OR_SetService<TestType>(_repository);
            _builder = new TestTypeBuilder(new Random());
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value, Guid tag)
        {
            var element = new OR_SetElement<TestType>(value, tag);
            _orSetService.Merge(new HashSet<OR_SetElement<TestType>> { element }.ToImmutableHashSet(), ImmutableHashSet<OR_SetElement<TestType>>.Empty);

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(HashSet<OR_SetElement<TestType>> values)
        {
            _orSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<OR_SetElement<TestType>>.Empty);

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SingleElement_AddsElementsToTheRepository(HashSet<OR_SetElement<TestType>> existingValues, TestType value, Guid tag)
        {
            _repository.PersistAdds(existingValues.ToImmutableHashSet());

            var element = new OR_SetElement<TestType>(value, tag);
            _orSetService.Merge(new HashSet<OR_SetElement<TestType>> { element }.ToImmutableHashSet(), ImmutableHashSet<OR_SetElement<TestType>>.Empty);

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElements_AddsElementsToTheRepository(HashSet<OR_SetElement<TestType>> existingValues, HashSet<OR_SetElement<TestType>> values)
        {
            _repository.PersistAdds(existingValues.ToImmutableHashSet());

            _orSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<OR_SetElement<TestType>>.Empty);

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsIdempotent(HashSet<OR_SetElement<TestType>> values, TestType value, Guid tag)
        {
            _repository.PersistAdds(values.ToImmutableHashSet());

            var element = new OR_SetElement<TestType>(value, tag);

            values.Add(element);

            _orSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<OR_SetElement<TestType>>.Empty);
            _orSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<OR_SetElement<TestType>>.Empty);
            _orSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<OR_SetElement<TestType>>.Empty);

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsCommutative(Guid firstTag, Guid secondTag)
        {
            var firstElement = new OR_SetElement<TestType>(_builder.Build(), firstTag);
            var secondElement = new OR_SetElement<TestType>(_builder.Build(), secondTag);
            var thirdElement = new OR_SetElement<TestType>(_builder.Build(), secondTag);
            var fourthElement = new OR_SetElement<TestType>(_builder.Build(), firstTag);
            var fifthElement = new OR_SetElement<TestType>(_builder.Build(), firstTag);

            var firstRepository = new OR_SetRepository();
            var firstService = new OR_SetService<TestType>(firstRepository);

            _repository.PersistAdds(new HashSet<OR_SetElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet());
            firstService.Merge(new HashSet<OR_SetElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet(), ImmutableHashSet<OR_SetElement<TestType>>.Empty);

            var firstRepositoryValues = firstRepository.GetAdds();

            var secondRepository = new OR_SetRepository();
            var secondService = new OR_SetService<TestType>(secondRepository);

            _repository.PersistAdds(new HashSet<OR_SetElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet());
            secondService.Merge(new HashSet<OR_SetElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet(), ImmutableHashSet<OR_SetElement<TestType>>.Empty);

            var secondRepositoryValues = firstRepository.GetAdds();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleValueWithEmptyRepository_AddsElementsToTheRepository(OR_SetElement<TestType> value)
        {
            _repository.PersistAdds(new HashSet<OR_SetElement<TestType>> { value }.ToImmutableHashSet());
            _orSetService.Merge(ImmutableHashSet<OR_SetElement<TestType>>.Empty, new HashSet<OR_SetElement<TestType>> { value }.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(HashSet<OR_SetElement<TestType>> values)
        {
            _repository.PersistAdds(values.ToImmutableHashSet());
            _orSetService.Merge(ImmutableHashSet<OR_SetElement<TestType>>.Empty, values.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleElement_AddsElementsToTheRepository(HashSet<OR_SetElement<TestType>> existingValues, OR_SetElement<TestType> value)
        {
            _repository.PersistRemoves(existingValues.ToImmutableHashSet());
            _repository.PersistAdds(new HashSet<OR_SetElement<TestType>> { value }.ToImmutableHashSet());

            _orSetService.Merge(ImmutableHashSet<OR_SetElement<TestType>>.Empty, new HashSet<OR_SetElement<TestType>> { value }.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SeveralElements_AddsElementsToTheRepository(HashSet<OR_SetElement<TestType>> existingValues, HashSet<OR_SetElement<TestType>> values)
        {
            _repository.PersistRemoves(existingValues.ToImmutableHashSet());
            _repository.PersistAdds(values.ToImmutableHashSet());

            _orSetService.Merge(ImmutableHashSet<OR_SetElement<TestType>>.Empty, values.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_IsIdempotent(HashSet<OR_SetElement<TestType>> values, OR_SetElement<TestType> value)
        {
            _repository.PersistRemoves(values.ToImmutableHashSet());
            _repository.PersistAdds(new HashSet<OR_SetElement<TestType>> { value }.ToImmutableHashSet());

            values.Add(value);

            _orSetService.Merge(ImmutableHashSet<OR_SetElement<TestType>>.Empty, values.ToImmutableHashSet());
            _orSetService.Merge(ImmutableHashSet<OR_SetElement<TestType>>.Empty, values.ToImmutableHashSet());
            _orSetService.Merge(ImmutableHashSet<OR_SetElement<TestType>>.Empty, values.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_IsCommutative(Guid firstTag, Guid secondTag)
        {
            var firstElement = new OR_SetElement<TestType>(_builder.Build(), firstTag);
            var secondElement = new OR_SetElement<TestType>(_builder.Build(), secondTag);
            var thirdElement = new OR_SetElement<TestType>(_builder.Build(), secondTag);
            var fourthElement = new OR_SetElement<TestType>(_builder.Build(), firstTag);
            var fifthElement = new OR_SetElement<TestType>(_builder.Build(), firstTag);

            var firstRepository = new OR_SetRepository();
            var firstService = new OR_SetService<TestType>(firstRepository);

            firstRepository.PersistRemoves(new HashSet<OR_SetElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet());
            firstRepository.PersistAdds(new HashSet<OR_SetElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet());
            firstService.Merge(ImmutableHashSet<OR_SetElement<TestType>>.Empty, new HashSet<OR_SetElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet());

            var firstRepositoryValues = firstRepository.GetRemoves();

            var secondRepository = new OR_SetRepository();
            var secondService = new OR_SetService<TestType>(secondRepository);

            secondRepository.PersistRemoves(new HashSet<OR_SetElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet());
            secondRepository.PersistAdds(new HashSet<OR_SetElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet());
            secondService.Merge(ImmutableHashSet<OR_SetElement<TestType>>.Empty, new HashSet<OR_SetElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet());

            var secondRepositoryValues = firstRepository.GetRemoves();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }


        [Theory]
        [AutoData]
        public void Lookup_SingleElementAdded_ReturnsTrue(OR_SetElement<TestType> value)
        {
            _orSetService.Merge(new HashSet<OR_SetElement<TestType>> { value }.ToImmutableHashSet(), ImmutableHashSet<OR_SetElement<TestType>>.Empty);

            var lookup = _orSetService.Lookup(value.Value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SeveralElementsWithDifferentTags_ReturnsTrue(TestType value, Guid firstTag, Guid secondTag)
        {
            var elements = new HashSet<OR_SetElement<TestType>>
            {
                new (value, firstTag),
                new (value, secondTag)
            };
            _orSetService.Merge(elements.ToImmutableHashSet(), ImmutableHashSet<OR_SetElement<TestType>>.Empty);

            var lookup = _orSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_NonExistingElement_ReturnsFalse(OR_SetElement<TestType> value)
        {
            _orSetService.Merge(new HashSet<OR_SetElement<TestType>> { value }.ToImmutableHashSet(), new HashSet<OR_SetElement<TestType>> { value }.ToImmutableHashSet());

            var lookup = _orSetService.Lookup(value.Value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AllTagsRemoved_ReturnsFalse(TestType value, Guid firstTag, Guid secondTag)
        {
            var elements = new HashSet<OR_SetElement<TestType>>
            {
                new (value, firstTag),
                new (value, secondTag)
            };
            _orSetService.Merge(elements.ToImmutableHashSet(), elements.ToImmutableHashSet());

            var lookup = _orSetService.Lookup(value);

            Assert.False(lookup);
        }

        private void AssertContains(HashSet<OR_SetElement<TestType>> expectedValues, IEnumerable<OR_SetElement<TestType>> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}