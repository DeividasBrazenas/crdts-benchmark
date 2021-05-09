using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Commutative.Set;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Application.UnitTests.Commutative
{
    public class P_OptimizedSetServiceTests
    {
        private readonly IP_OptimizedSetRepository<TestType> _repository;
        private readonly P_OptimizedSetService<TestType> _pSetService;

        public P_OptimizedSetServiceTests()
        {
            _repository = new P_OptimizedSetRepository();
            _pSetService = new P_OptimizedSetService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void Add_NoExistingValues_AddsElementToTheRepository(TestType value)
        {
            _pSetService.DownstreamAdd(value);

            var repositoryValues = _repository.GetElements();
            Assert.Contains(new P_OptimizedSetElement<TestType>(value, false), repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(HashSet<P_OptimizedSetElement<TestType>> elements, TestType value)
        {
            _repository.PersistElements(elements.ToImmutableHashSet());

            _pSetService.DownstreamAdd(value);

            var repositoryValues = _repository.GetElements();
            Assert.Contains(new P_OptimizedSetElement<TestType>(value, false), repositoryValues);
        }     
        
        [Theory]
        [AutoData]
        public void Add_IsIdempotent(HashSet<P_OptimizedSetElement<TestType>> elements, TestType value)
        {
            _repository.PersistElements(elements.ToImmutableHashSet());

            _pSetService.DownstreamAdd(value);
            _pSetService.DownstreamAdd(value);
            _pSetService.DownstreamAdd(value);

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(v => Equals(v, new P_OptimizedSetElement<TestType>(value, false))));
        }

        [Theory]
        [AutoData]
        public void Remove_AddDoesNotExist_DoesNotAddElementToTheRepository(TestType value)
        {
            _pSetService.DownstreamRemove(value);

            var repositoryValues = _repository.GetElements();
            Assert.DoesNotContain(new P_OptimizedSetElement<TestType>(value, true), repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddExists_AddsElementToTheRepository(TestType value)
        {
            _pSetService.DownstreamAdd(value);
            _pSetService.DownstreamRemove(value);

            var repositoryValues = _repository.GetElements();
            Assert.Contains(new P_OptimizedSetElement<TestType>(value, true), repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Remove_IsIdempotent(TestType value)
        {
            _pSetService.DownstreamAdd(value);
            _pSetService.DownstreamRemove(value);
            _pSetService.DownstreamRemove(value);
            _pSetService.DownstreamRemove(value);

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(v => Equals(v, new P_OptimizedSetElement<TestType>(value, true))));
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsTrue(TestType value)
        {
            _pSetService.DownstreamAdd(value);

            var lookup = _pSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsFalse(TestType value)
        {
            _pSetService.DownstreamAdd(value);
            _pSetService.DownstreamRemove(value);

            var lookup = _pSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdd_ReturnsFalse(TestType value)
        {
            _pSetService.DownstreamAdd(value);
            _pSetService.DownstreamRemove(value);
            _pSetService.DownstreamAdd(value);

            var lookup = _pSetService.Lookup(value);

            Assert.False(lookup);
        }
    }
}