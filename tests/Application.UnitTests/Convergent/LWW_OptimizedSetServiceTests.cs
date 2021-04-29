using System.Collections.Generic;
using AutoFixture.Xunit2;
using CRDT.Application.Convergent;
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
            _lwwSetService.Merge(new List<LWW_OptimizedSetElement<TestType>> { element });

            var repositoryValues = _repository.GetElements();
            Assert.Contains(element, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(List<LWW_OptimizedSetElement<TestType>> elements, TestType value, long timestamp)
        {
            _repository.PersistElements(elements);

            var element = new LWW_OptimizedSetElement<TestType>(value, timestamp, false);
            _lwwSetService.Merge(new List<LWW_OptimizedSetElement<TestType>> { element });

            var repositoryValues = _repository.GetElements();
            Assert.Contains(element, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddExistsWithLowerTimestamp_AddsElementToTheRepository(TestType value, long timestamp)
        {
            var addElement = new LWW_OptimizedSetElement<TestType>(value, timestamp, false);
            var removeElement = new LWW_OptimizedSetElement<TestType>(value, timestamp + 10, true);

            _lwwSetService.Merge(new List<LWW_OptimizedSetElement<TestType>> { addElement });
            _lwwSetService.Merge(new List<LWW_OptimizedSetElement<TestType>> { removeElement });

            var repositoryValues = _repository.GetElements();

            Assert.Single(repositoryValues);
            Assert.Contains(removeElement, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Merge_IsIdempotent(TestType value, long timestamp)
        {
            var addElement = new LWW_OptimizedSetElement<TestType>(value, timestamp, false);
            var removeElement = new LWW_OptimizedSetElement<TestType>(value, timestamp + 10, true);

            _lwwSetService.Merge(new List<LWW_OptimizedSetElement<TestType>> { addElement });
            _lwwSetService.Merge(new List<LWW_OptimizedSetElement<TestType>> { removeElement });
            _lwwSetService.Merge(new List<LWW_OptimizedSetElement<TestType>> { removeElement });
            _lwwSetService.Merge(new List<LWW_OptimizedSetElement<TestType>> { removeElement });

            var repositoryValues = _repository.GetElements();
        
            Assert.Single(repositoryValues);
            Assert.Contains(removeElement, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Lookup_Added_ReturnsTrue(TestType value, long timestamp)
        {
            var addElement = new LWW_OptimizedSetElement<TestType>(value, timestamp, false);

            _lwwSetService.Merge(new List<LWW_OptimizedSetElement<TestType>> { addElement });

            var lookup = _lwwSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_Removed_ReturnsFalse(TestType value, long timestamp)
        {
            var addElement = new LWW_OptimizedSetElement<TestType>(value, timestamp, false);
            var removeElement = new LWW_OptimizedSetElement<TestType>(value, timestamp + 10, true);

            _lwwSetService.Merge(new List<LWW_OptimizedSetElement<TestType>> { addElement });
            _lwwSetService.Merge(new List<LWW_OptimizedSetElement<TestType>> { removeElement });

            var lookup = _lwwSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsTrue(TestType value, long timestamp)
        {
            var addElement = new LWW_OptimizedSetElement<TestType>(value, timestamp, false);
            var removeElement = new LWW_OptimizedSetElement<TestType>(value, timestamp + 10, true);
            var reAddElement = new LWW_OptimizedSetElement<TestType>(value, timestamp + 20, false);

            _lwwSetService.Merge(new List<LWW_OptimizedSetElement<TestType>> { addElement });
            _lwwSetService.Merge(new List<LWW_OptimizedSetElement<TestType>> { removeElement });
            _lwwSetService.Merge(new List<LWW_OptimizedSetElement<TestType>> { reAddElement });

            var lookup = _lwwSetService.Lookup(value);

            Assert.True(lookup);
        }
    }
}