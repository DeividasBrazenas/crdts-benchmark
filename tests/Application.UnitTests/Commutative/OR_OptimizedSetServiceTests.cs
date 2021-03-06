using System;
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
        public void Add_NoExistingValues_AddsElementToTheRepository(TestType value, Guid tag)
        {
            _orSetService.DownstreamAdd(value, tag);

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == tag);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(HashSet<OR_OptimizedSetElement<TestType>> adds, TestType value, Guid tag)
        {
            _repository.PersistElements(adds.ToImmutableHashSet());

            _orSetService.DownstreamAdd(value, tag);

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == tag);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithDifferentTag_AddsElementToTheRepository(HashSet<OR_OptimizedSetElement<TestType>> adds, TestType value, Guid tag, Guid otherNode)
        {
            _repository.PersistElements(adds.ToImmutableHashSet());

            _orSetService.DownstreamAdd(value, tag);
            _orSetService.DownstreamAdd(value, otherNode);

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Equal(2, actualValues.Count());
        }

        [Theory]
        [AutoData]
        public void Add_IsIdempotent(TestType value, Guid tag)
        {
            _orSetService.DownstreamAdd(value, tag);
            _orSetService.DownstreamAdd(value, tag);
            _orSetService.DownstreamAdd(value, tag);

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddDoesNotExist_DoesNotAddElementToTheRepository(TestType value, Guid tag)
        {
            _orSetService.DownstreamRemove(value, new[] { tag });

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Empty(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddExists_AddsElementToTheRepository(TestType value, Guid tag)
        {
            _orSetService.DownstreamAdd(value, tag);
            _orSetService.DownstreamRemove(value, new[] { tag });

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == tag);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_IsIdempotent(TestType value, Guid tag)
        {
            _orSetService.DownstreamAdd(value, tag);
            _orSetService.DownstreamRemove(value, new[] { tag });
            _orSetService.DownstreamRemove(value, new[] { tag });
            _orSetService.DownstreamRemove(value, new[] { tag });

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == tag);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Lookup_SingleElementAdded_ReturnsTrue(TestType value, Guid tag)
        {
            _orSetService.DownstreamAdd(value, tag);

            var lookup = _orSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SeveralElementsWithDifferentTags_ReturnsTrue(TestType value, Guid tag, Guid otherTag)
        {
            _orSetService.DownstreamAdd(value, tag);
            _orSetService.DownstreamAdd(value, otherTag);

            var lookup = _orSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_NonExistingElement_ReturnsFalse(TestType value)
        {
            var lookup = _orSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AllTagsRemoved_ReturnsFalse(TestType value, Guid tag, Guid otherTag)
        {
            _orSetService.DownstreamAdd(value, tag);
            _orSetService.DownstreamAdd(value, otherTag);
            _orSetService.DownstreamRemove(value, new []{tag, otherTag});

            var lookup = _orSetService.Lookup(value);

            Assert.False(lookup);
        }
    }
}