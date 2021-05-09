using System.Collections.Generic;
using System.Collections.Immutable;
using AutoFixture.Xunit2;
using CRDT.Application.Convergent.Set;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Application.UnitTests.Convergent
{
    public class LWW_OptimizedSetServiceTests
    {
        private readonly ILWW_OptimizedSetRepository<TestType> _repository;
        private readonly LWW_OptimizedSetService<TestType> _lwwSetService;

        public LWW_OptimizedSetServiceTests()
        {
            _repository = new LWW_OptimizedSetRepository();
            _lwwSetService = new LWW_OptimizedSetService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void Add_NoExistingValues_AddsElementToTheRepository(TestType value, long timestamp)
        {
            var element = new LWW_OptimizedSetElement<TestType>(value, timestamp, false);
            _lwwSetService.Merge(new HashSet<LWW_OptimizedSetElement<TestType>> { element }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Contains(element, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(HashSet<LWW_OptimizedSetElement<TestType>> elements, TestType value, long timestamp)
        {
            _repository.PersistElements(elements.ToImmutableHashSet());

            var element = new LWW_OptimizedSetElement<TestType>(value, timestamp, false);
            _lwwSetService.Merge(new HashSet<LWW_OptimizedSetElement<TestType>> { element }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Contains(element, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddExistsWithLowerTimestamp_AddsElementToTheRepository(TestType value, long timestamp)
        {
            var removeElement = new LWW_OptimizedSetElement<TestType>(value, timestamp + 10, true);

            _lwwSetService.LocalAssign(value, timestamp);
            _lwwSetService.LocalRemove(value, timestamp + 10);

            var repositoryValues = _repository.GetElements();

            Assert.Single(repositoryValues);
            Assert.Contains(removeElement, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Merge_IsIdempotent(TestType value, long timestamp)
        {
            var removeElement = new LWW_OptimizedSetElement<TestType>(value, timestamp, true);

            _lwwSetService.Merge(new HashSet<LWW_OptimizedSetElement<TestType>> { removeElement }.ToImmutableHashSet());
            _lwwSetService.Merge(new HashSet<LWW_OptimizedSetElement<TestType>> { removeElement }.ToImmutableHashSet());
            _lwwSetService.Merge(new HashSet<LWW_OptimizedSetElement<TestType>> { removeElement }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
        
            Assert.Single(repositoryValues);
            Assert.Contains(removeElement, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Lookup_Added_ReturnsTrue(TestType value, long timestamp)
        {
            var addElement = new LWW_OptimizedSetElement<TestType>(value, timestamp, false);

            _lwwSetService.Merge(new HashSet<LWW_OptimizedSetElement<TestType>> { addElement }.ToImmutableHashSet());

            var lookup = _lwwSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_Removed_ReturnsFalse(TestType value, long timestamp)
        {
            _lwwSetService.LocalAssign(value, timestamp);
            _lwwSetService.LocalRemove(value, timestamp + 10);

            var lookup = _lwwSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsTrue(TestType value, long timestamp)
        {
            _lwwSetService.LocalAssign(value, timestamp);
            _lwwSetService.LocalRemove(value, timestamp + 10);
            _lwwSetService.LocalAssign(value, timestamp + 20);

            var lookup = _lwwSetService.Lookup(value);

            Assert.True(lookup);
        }
    }
}