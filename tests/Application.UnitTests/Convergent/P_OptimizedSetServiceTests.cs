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
    public class P_OptimizedSetServiceTests
    {
        private readonly IP_OptimizedSetRepository<TestType> _repository;
        private readonly P_OptimizedSetService<TestType> _pSetService;
        private readonly TestTypeBuilder _builder;
        public P_OptimizedSetServiceTests()
        {
            _repository = new P_OptimizedSetRepository();
            _pSetService = new P_OptimizedSetService<TestType>(_repository);
            _builder = new TestTypeBuilder(new Random());
        }

        [Theory]
        [AutoData]
        public void Merge_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value)
        {
            _pSetService.Merge(new HashSet<P_OptimizedSetElement<TestType>> { new(value, false) }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Merge_SingleElement_AddsElementsToTheRepository(HashSet<P_OptimizedSetElement<TestType>> existingValues, TestType value)
        {
            _repository.PersistElements(existingValues.ToImmutableHashSet());

            _pSetService.Merge(new HashSet<P_OptimizedSetElement<TestType>> { new(value, false) }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Merge_IsIdempotent(HashSet<P_OptimizedSetElement<TestType>> values, P_OptimizedSetElement<TestType> value)
        {
            _repository.PersistElements(values.ToImmutableHashSet());

            values.Add(value);

            _pSetService.Merge(values.ToImmutableHashSet());
            _pSetService.Merge(values.ToImmutableHashSet());
            _pSetService.Merge(values.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Fact]
        public void Merge_IsCommutative()
        {
            var firstValue = new P_OptimizedSetElement<TestType>(_builder.Build(), false);
            var secondValue = new P_OptimizedSetElement<TestType>(_builder.Build(), true);
            var thirdValue = new P_OptimizedSetElement<TestType>(_builder.Build(), false);
            var fourthValue = new P_OptimizedSetElement<TestType>(_builder.Build(), true);
            var fifthValue = new P_OptimizedSetElement<TestType>(_builder.Build(), false);

            var firstRepository = new P_OptimizedSetRepository();
            var firstService = new P_OptimizedSetService<TestType>(firstRepository);

            _repository.PersistElements(new HashSet<P_OptimizedSetElement<TestType>> { firstValue, secondValue, thirdValue }.ToImmutableHashSet());
            firstService.Merge(new HashSet<P_OptimizedSetElement<TestType>> { fourthValue, fifthValue }.ToImmutableHashSet());

            var firstRepositoryValues = firstRepository.GetElements();

            var secondRepository = new P_OptimizedSetRepository();
            var secondService = new P_OptimizedSetService<TestType>(secondRepository);

            _repository.PersistElements(new HashSet<P_OptimizedSetElement<TestType>> { fourthValue, fifthValue }.ToImmutableHashSet());
            secondService.Merge(new HashSet<P_OptimizedSetElement<TestType>> { firstValue, secondValue, thirdValue }.ToImmutableHashSet());

            var secondRepositoryValues = firstRepository.GetElements();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleValueWithEmptyRepository_UpdatesElementInRepository(TestType value)
        {
            _repository.PersistElements(new HashSet<P_OptimizedSetElement<TestType>> { new(value, false) }.ToImmutableHashSet());

            var removeElement = new P_OptimizedSetElement<TestType>(value, true);
            _pSetService.LocalRemove(value);

            _pSetService.Merge(_pSetService.State);

            var repositoryValues = _repository.GetElements();

            Assert.Single(repositoryValues);
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, removeElement)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_IsIdempotent(TestType value)
        {
            _repository.PersistElements(new HashSet<P_OptimizedSetElement<TestType>> { new(value, false) }.ToImmutableHashSet());

            var removeElement = new P_OptimizedSetElement<TestType>(value, true);
            _pSetService.LocalRemove(value);

            _pSetService.Merge(_pSetService.State);
            _pSetService.Merge(_pSetService.State);
            _pSetService.Merge(_pSetService.State);

            var repositoryValues = _repository.GetElements();

            Assert.Single(repositoryValues);
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, removeElement)));
        }


        [Theory]
        [AutoData]
        public void Lookup_ReturnsTrue(TestType value)
        {
            var element = new P_OptimizedSetElement<TestType>(value, false);

            _pSetService.Merge(new HashSet<P_OptimizedSetElement<TestType>> { element }.ToImmutableHashSet());

            var lookup = _pSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsFalse(TestType value)
        {
            _pSetService.LocalAdd(value);
            _pSetService.Merge(_pSetService.State);

            _pSetService.LocalRemove(value);
            _pSetService.Merge(_pSetService.State);

            var lookup = _pSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdd_ReturnsFalse(TestType value)
        {
            _pSetService.LocalAdd(value);
            _pSetService.Merge(_pSetService.State);

            _pSetService.LocalRemove(value);
            _pSetService.Merge(_pSetService.State);

            _pSetService.LocalAdd(value);
            _pSetService.Merge(_pSetService.State);

            var lookup = _pSetService.Lookup(value);

            Assert.False(lookup);
        }

        private void AssertContains(HashSet<P_OptimizedSetElement<TestType>> expectedValues, IEnumerable<P_OptimizedSetElement<TestType>> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}