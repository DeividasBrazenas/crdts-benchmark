using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Commutative.Set;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;
using static CRDT.UnitTestHelpers.TestTypes.TestTypeBuilder;

namespace CRDT.Application.UnitTests.Commutative
{
    public class OUR_OptimizedSetWithVCServiceTests
    {
        private readonly IOUR_OptimizedSetWithVCRepository<TestType> _repository;
        private readonly OUR_OptimizedSetWithVCService<TestType> _ourSetService;
        private readonly TestTypeBuilder _builder;

        public OUR_OptimizedSetWithVCServiceTests()
        {
            _repository = new OUR_OptimizedSetWithVCRepository();
            _ourSetService = new OUR_OptimizedSetWithVCService<TestType>(_repository);
            _builder = new TestTypeBuilder(new Random());
        }

        [Theory]
        [AutoData]
        public void Add_NoExistingValues_AddsElementToTheRepository(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.DownstreamAdd(value, tag, new VectorClock(clock.Add(node, 0)));

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == tag && v.VectorClock.Equals(new VectorClock(clock.Add(node, 0))));

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(HashSet<OUR_OptimizedSetWithVCElement<TestType>> adds, TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistElements(adds.ToImmutableHashSet());

            _ourSetService.DownstreamAdd(value, tag, new VectorClock(clock.Add(node, 0)));

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == tag && v.VectorClock.Equals(new VectorClock(clock.Add(node, 0))));

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithDifferentTag_AddsElementToTheRepository(HashSet<OUR_OptimizedSetWithVCElement<TestType>> adds, TestType value, Guid tag, Guid otherTag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistElements(adds.ToImmutableHashSet());

            _ourSetService.DownstreamAdd(value, tag, new VectorClock(clock.Add(node, 0)));
            _ourSetService.DownstreamAdd(value, otherTag, new VectorClock(clock.Add(node, 0)));

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Equal(2, actualValues.Count());
        }

        [Theory]
        [AutoData]
        public void Add_IsIdempotent(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.DownstreamAdd(value, tag, new VectorClock(clock.Add(node, 0)));
            _ourSetService.DownstreamAdd(value, tag, new VectorClock(clock.Add(node, 0)));
            _ourSetService.DownstreamAdd(value, tag, new VectorClock(clock.Add(node, 0)));

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddDoesNotExist_DoesNotAddElementToTheRepository(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.DownstreamRemove(value, new[] { tag }, new VectorClock(clock.Add(node, 0)));

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Empty(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddExists_AddsElementToTheRepository(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.DownstreamAdd(value, tag, new VectorClock(clock.Add(node, 0)));
            _ourSetService.DownstreamRemove(value, new[] { tag }, new VectorClock(clock.Add(node, 0)));

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == tag && v.Removed);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_IsIdempotent(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.DownstreamAdd(value, tag, new VectorClock(clock.Add(node, 0)));
            _ourSetService.DownstreamRemove(value, new[] { tag }, new VectorClock(clock.Add(node, 0)));
            _ourSetService.DownstreamRemove(value, new[] { tag }, new VectorClock(clock.Add(node, 0)));
            _ourSetService.DownstreamRemove(value, new[] { tag }, new VectorClock(clock.Add(node, 0)));

            var repositoryValues = _repository.GetElements();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == tag && v.Removed);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Lookup_SingleElementAdded_ReturnsTrue(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.DownstreamAdd(value, tag, new VectorClock(clock.Add(node, 0)));

            var lookup = _ourSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SeveralElementsWithDifferentTags_ReturnsTrue(TestType value, Guid tag, Guid otherTag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.DownstreamAdd(value, tag, new VectorClock(clock.Add(node, 0)));
            _ourSetService.DownstreamAdd(value, otherTag, new VectorClock(clock.Add(node, 5)));

            var lookup = _ourSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_NonExistingElement_ReturnsFalse(TestType value)
        {
            var lookup = _ourSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AllTagsRemoved_ReturnsFalse(TestType value, Guid tag, Guid otherTag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.DownstreamAdd(value, tag, new VectorClock(clock.Add(node, 0)));
            _ourSetService.DownstreamAdd(value, otherTag, new VectorClock(clock.Add(node, 0)));
            _ourSetService.DownstreamRemove(value, new[] { tag, otherTag }, new VectorClock(clock.Add(node, 0)));

            var lookup = _ourSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_UpdatedElement_ReturnsTrueForUpdatedElement(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.DownstreamAdd(value, tag, new VectorClock(clock.Add(node, 0)));

            var newValue = _builder.Build(value.Id);
            _ourSetService.DownstreamUpdate(newValue, new[] { tag }, new VectorClock(clock.Add(node, 3)));

            var lookup = _ourSetService.Lookup(newValue);
            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_UpdatedElement_ReturnsFalseForOldElement(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.DownstreamAdd(value, tag, new VectorClock(clock.Add(node, 0)));

            var newValue = _builder.Build(value.Id);
            _ourSetService.DownstreamUpdate(newValue, new[] { tag }, new VectorClock(clock.Add(node, 3)));

            var lookup = _ourSetService.Lookup(value);
            Assert.False(lookup);
        }
    }
}