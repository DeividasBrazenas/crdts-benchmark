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

namespace CRDT.Application.UnitTests.Commutative
{
    public class LWW_SetWithVCServiceTests
    {
        private readonly ILWW_SetWithVCRepository<TestType> _repository;
        private readonly LWW_SetWithVCService<TestType> _lwwSetService;

        public LWW_SetWithVCServiceTests()
        {
            _repository = new LWW_SetWithVCRepository();
            _lwwSetService = new LWW_SetWithVCService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void Add_NoExistingValues_AddsElementToTheRepository(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _lwwSetService.Add(value, new VectorClock(clock.Add(node, 0)));

            var repositoryValues = _repository.GetAdds();
            Assert.Contains(value, repositoryValues.Select(v => v.Value));
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(List<LWW_SetWithVCElement<TestType>> adds, TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistAdds(adds);

            _lwwSetService.Add(value, new VectorClock(clock.Add(node, 0)));

            var repositoryValues = _repository.GetAdds();
            Assert.Contains(value, repositoryValues.Select(v => v.Value));
        }

        [Theory]
        [AutoData]
        public void Remove_AddDoesNotExist_DoesNotAddElementToTheRepository(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _lwwSetService.Remove(value, new VectorClock(clock.Add(node, 0)));

            var repositoryValues = _repository.GetRemoves();
            Assert.DoesNotContain(value, repositoryValues.Select(v => v.Value));
        }

        [Theory]
        [AutoData]
        public void Remove_AddExistsWithLowerTimestamp_AddsElementToTheRepository(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _lwwSetService.Add(value, new VectorClock(clock.Add(node, 0)));
            _lwwSetService.Remove(value, new VectorClock(clock.Add(node, 1)));

            var repositoryValues = _repository.GetRemoves();
            Assert.Contains(value, repositoryValues.Select(v => v.Value));
        }

        [Theory]
        [AutoData]
        public void Lookup_Added_ReturnsTrue(List<LWW_SetWithVCElement<TestType>> existingAdds, List<LWW_SetWithVCElement<TestType>> existingRemoves, TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistAdds(existingAdds);
            _repository.PersistRemoves(existingRemoves);

            _lwwSetService.Add(value, new VectorClock(clock.Add(node, 0)));

            var lookup = _lwwSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_Removed_ReturnsFalse(List<LWW_SetWithVCElement<TestType>> existingAdds, List<LWW_SetWithVCElement<TestType>> existingRemoves, TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistAdds(existingAdds);
            _repository.PersistRemoves(existingRemoves);

            _lwwSetService.Add(value, new VectorClock(clock.Add(node, 0)));
            _lwwSetService.Remove(value, new VectorClock(clock.Add(node, 1)));

            var lookup = _lwwSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsTrue(List<LWW_SetWithVCElement<TestType>> existingAdds, List<LWW_SetWithVCElement<TestType>> existingRemoves, TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistAdds(existingAdds);
            _repository.PersistRemoves(existingRemoves);

            _lwwSetService.Add(value, new VectorClock(clock.Add(node, 0)));
            _lwwSetService.Remove(value, new VectorClock(clock.Add(node, 1)));
            _lwwSetService.Add(value, new VectorClock(clock.Add(node, 2)));

            var lookup = _lwwSetService.Lookup(value);

            Assert.True(lookup);
        }
    }
}