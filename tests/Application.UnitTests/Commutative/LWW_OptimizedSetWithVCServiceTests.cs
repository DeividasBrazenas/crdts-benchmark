using System.Collections.Generic;
using System.Collections.Immutable;
using AutoFixture.Xunit2;
using CRDT.Application.Commutative.Set;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Application.UnitTests.Commutative
{
    public class LWW_OptimizedSetWithVCServiceTests
    {
        private readonly ILWW_OptimizedSetWithVCRepository<TestType> _repository;
        private readonly LWW_OptimizedSetWithVCService<TestType> _lwwSetService;

        public LWW_OptimizedSetWithVCServiceTests()
        {
            _repository = new LWW_OptimizedSetWithVCRepository();
            _lwwSetService = new LWW_OptimizedSetWithVCService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void Add_NoExistingValues_AddsElementToTheRepository(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _lwwSetService.DownstreamAssign(value, new VectorClock(clock.Add(node, 0)));

            var repositoryValues = _repository.GetElements();

            var element = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false);
            Assert.Contains(element, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(HashSet<LWW_OptimizedSetWithVCElement<TestType>> elements, TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistElements(elements.ToImmutableHashSet());

            _lwwSetService.DownstreamAssign(value, new VectorClock(clock.Add(node, 0)));

            var repositoryValues = _repository.GetElements();

            var element = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false);
            Assert.Contains(element, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddExistsWithLowerTimestamp_AddsElementToTheRepository(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _lwwSetService.DownstreamAssign(value, new VectorClock(clock.Add(node, 0)));
            _lwwSetService.DownstreamRemove(value, new VectorClock(clock.Add(node, 1)));

            var repositoryValues = _repository.GetElements();

            var element = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)), true);
            Assert.Contains(element, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Lookup_Added_ReturnsTrue(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _lwwSetService.DownstreamAssign(value, new VectorClock(clock.Add(node, 0)));

            var lookup = _lwwSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_Removed_ReturnsFalse(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _lwwSetService.DownstreamAssign(value, new VectorClock(clock.Add(node, 0)));
            _lwwSetService.DownstreamRemove(value, new VectorClock(clock.Add(node, 1)));

            var lookup = _lwwSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsTrue(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _lwwSetService.DownstreamAssign(value, new VectorClock(clock.Add(node, 0)));
            _lwwSetService.DownstreamRemove(value, new VectorClock(clock.Add(node, 1)));
            _lwwSetService.DownstreamAssign(value, new VectorClock(clock.Add(node, 2)));

            var lookup = _lwwSetService.Lookup(value);

            Assert.True(lookup);
        }
    }
}