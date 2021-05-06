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
using static CRDT.UnitTestHelpers.TestTypes.TestTypeBuilder;

namespace CRDT.Application.UnitTests.Convergent
{
    public class U_SetServiceTests
    {
        private readonly IU_SetRepository<TestType> _repository;
        private readonly U_SetService<TestType> _uSetService;
        private readonly TestTypeBuilder _builder;
        public U_SetServiceTests()
        {
            _repository = new U_SetRepository();
            _uSetService = new U_SetService<TestType>(_repository);
            _builder = new TestTypeBuilder(new Random());
        }

        [Theory]
        [AutoData]
        public void Merge_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value)
        {
            _uSetService.Merge(new HashSet<U_SetElement<TestType>> { new(value, false) }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Merge_SingleElement_AddsElementsToTheRepository(HashSet<U_SetElement<TestType>> existingValues, TestType value)
        {
            _repository.PersistElements(existingValues.ToImmutableHashSet());

            _uSetService.Merge(new HashSet<U_SetElement<TestType>> { new(value, false) }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Merge_IsIdempotent(HashSet<U_SetElement<TestType>> values, U_SetElement<TestType> value)
        {
            _repository.PersistElements(values.ToImmutableHashSet());

            values.Add(value);

            _uSetService.Merge(values.ToImmutableHashSet());
            _uSetService.Merge(values.ToImmutableHashSet());
            _uSetService.Merge(values.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Fact]
        public void Merge_IsCommutative()
        {
            var firstValue = new U_SetElement<TestType>(_builder.Build(), false);
            var secondValue = new U_SetElement<TestType>(_builder.Build(), true);
            var thirdValue = new U_SetElement<TestType>(_builder.Build(), false);
            var fourthValue = new U_SetElement<TestType>(_builder.Build(), true);
            var fifthValue = new U_SetElement<TestType>(_builder.Build(), false);

            var firstRepository = new U_SetRepository();
            var firstService = new U_SetService<TestType>(firstRepository);

            _repository.PersistElements(new HashSet<U_SetElement<TestType>> { firstValue, secondValue, thirdValue }.ToImmutableHashSet());
            firstService.Merge(new HashSet<U_SetElement<TestType>> { fourthValue, fifthValue }.ToImmutableHashSet());

            var firstRepositoryValues = firstRepository.GetElements();

            var secondRepository = new U_SetRepository();
            var secondService = new U_SetService<TestType>(secondRepository);

            _repository.PersistElements(new HashSet<U_SetElement<TestType>> { fourthValue, fifthValue }.ToImmutableHashSet());
            secondService.Merge(new HashSet<U_SetElement<TestType>> { firstValue, secondValue, thirdValue }.ToImmutableHashSet());

            var secondRepositoryValues = firstRepository.GetElements();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleValueWithEmptyRepository_UpdatesElementInRepository(TestType value)
        {
            _repository.PersistElements(new HashSet<U_SetElement<TestType>> { new(value, false) }.ToImmutableHashSet());

            var removeElement = new U_SetElement<TestType>(value, true);
            _uSetService.LocalRemove(value);

            _uSetService.Merge(_uSetService.State);

            var repositoryValues = _repository.GetElements();

            Assert.Single(repositoryValues);
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, removeElement)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_IsIdempotent(TestType value)
        {
            _repository.PersistElements(new HashSet<U_SetElement<TestType>> { new(value, false) }.ToImmutableHashSet());

            var removeElement = new U_SetElement<TestType>(value, true);
            _uSetService.LocalRemove(value);

            _uSetService.Merge(_uSetService.State);
            _uSetService.Merge(_uSetService.State);
            _uSetService.Merge(_uSetService.State);

            var repositoryValues = _repository.GetElements();

            Assert.Single(repositoryValues);
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, removeElement)));
        }


        [Theory]
        [AutoData]
        public void Lookup_ReturnsTrue(TestType value)
        {
            var element = new U_SetElement<TestType>(value, false);

            _uSetService.Merge(new HashSet<U_SetElement<TestType>> { element }.ToImmutableHashSet());

            var lookup = _uSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsFalse(TestType value)
        {
            _uSetService.LocalAdd(value);
            _uSetService.Merge(_uSetService.State);

            _uSetService.LocalRemove(value);
            _uSetService.Merge(_uSetService.State);

            var lookup = _uSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdd_ReturnsFalse(TestType value)
        {
            _uSetService.LocalAdd(value);
            _uSetService.Merge(_uSetService.State);

            _uSetService.LocalRemove(value);
            _uSetService.Merge(_uSetService.State);

            _uSetService.LocalAdd(value);
            _uSetService.Merge(_uSetService.State);

            var lookup = _uSetService.Lookup(value);

            Assert.False(lookup);
        }

        private void AssertContains(HashSet<U_SetElement<TestType>> expectedValues, IEnumerable<U_SetElement<TestType>> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}