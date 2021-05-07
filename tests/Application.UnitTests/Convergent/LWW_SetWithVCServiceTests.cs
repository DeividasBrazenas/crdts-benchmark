using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Convergent.Set;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Application.UnitTests.Convergent
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
        public void Add_NoExistingValues_AddsElementToTheRepository(LWW_SetWithVCElement<TestType> element)
        {
            _lwwSetService.Merge(new HashSet<LWW_SetWithVCElement<TestType>> { element }.ToImmutableHashSet(), ImmutableHashSet<LWW_SetWithVCElement<TestType>>.Empty);

            var repositoryValues = _repository.GetAdds();
            Assert.Contains(element, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(HashSet<LWW_SetWithVCElement<TestType>> adds, LWW_SetWithVCElement<TestType> element)
        {
            _repository.PersistAdds(adds.ToImmutableHashSet());

            _lwwSetService.Merge(new HashSet<LWW_SetWithVCElement<TestType>> { element }.ToImmutableHashSet(), ImmutableHashSet<LWW_SetWithVCElement<TestType>>.Empty);

            var repositoryValues = _repository.GetAdds();
            Assert.Contains(element, repositoryValues);
        }


        [Theory]
        [AutoData]
        public void Remove_AddExistsWithLowerTimestamp_AddsElementToTheRepository(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var addElement = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)));
            var removeElement = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)));

            _lwwSetService.Merge(new HashSet<LWW_SetWithVCElement<TestType>> { addElement }.ToImmutableHashSet(), new HashSet<LWW_SetWithVCElement<TestType>> { removeElement }.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            Assert.Contains(value, repositoryValues.Select(v => v.Value));
        }

        [Theory]
        [AutoData]
        public void Merge_NoExistingValues_AddsElementsToTheRepository(HashSet<LWW_SetWithVCElement<TestType>> values)
        {
            _lwwSetService.Merge(values.ToImmutableHashSet(), values.ToImmutableHashSet());

            var repositoryAdds = _repository.GetAdds();
            var repositoryRemoves = _repository.GetRemoves();

            AssertContains(values, repositoryAdds);
            AssertContains(values, repositoryRemoves);
        }

        [Theory]
        [AutoData]
        public void Merge_WithExistingValues_AddsElementsToTheRepository(HashSet<LWW_SetWithVCElement<TestType>> existingAdds, HashSet<LWW_SetWithVCElement<TestType>> existingRemoves, HashSet<LWW_SetWithVCElement<TestType>> values)
        {
            _repository.PersistAdds(existingAdds.ToImmutableHashSet());
            _repository.PersistRemoves(existingRemoves.ToImmutableHashSet());

            _lwwSetService.Merge(values.ToImmutableHashSet(), values.ToImmutableHashSet());

            var repositoryAdds = _repository.GetAdds();
            var repositoryRemoves = _repository.GetRemoves();

            AssertContains(values, repositoryAdds);
            AssertContains(values, repositoryRemoves);
        }

        [Theory]
        [AutoData]
        public void Merge_IsIdempotent(HashSet<LWW_SetWithVCElement<TestType>> existingAdds, HashSet<LWW_SetWithVCElement<TestType>> existingRemoves, HashSet<LWW_SetWithVCElement<TestType>> values)
        {
            _repository.PersistAdds(existingAdds.ToImmutableHashSet());
            _repository.PersistRemoves(existingRemoves.ToImmutableHashSet());

            _lwwSetService.Merge(values.ToImmutableHashSet(), values.ToImmutableHashSet());
            _lwwSetService.Merge(values.ToImmutableHashSet(), values.ToImmutableHashSet());
            _lwwSetService.Merge(values.ToImmutableHashSet(), values.ToImmutableHashSet());

            var repositoryAdds = _repository.GetAdds();
            var repositoryRemoves = _repository.GetRemoves();

            AssertContains(values, repositoryAdds);
            AssertContains(values, repositoryRemoves);
        }

        [Theory]
        [AutoData]
        public void Lookup_Added_ReturnsTrue(HashSet<LWW_SetWithVCElement<TestType>> existingAdds, HashSet<LWW_SetWithVCElement<TestType>> existingRemoves, LWW_SetWithVCElement<TestType> element)
        {
            _repository.PersistAdds(existingAdds.ToImmutableHashSet());
            _repository.PersistRemoves(existingRemoves.ToImmutableHashSet());

            _lwwSetService.Merge(new HashSet<LWW_SetWithVCElement<TestType>> { element }.ToImmutableHashSet(), ImmutableHashSet<LWW_SetWithVCElement<TestType>>.Empty);

            var lookup = _lwwSetService.Lookup(element.Value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_Removed_ReturnsFalse(HashSet<LWW_SetWithVCElement<TestType>> existingAdds, HashSet<LWW_SetWithVCElement<TestType>> existingRemoves, TestType value, Node node)
        {
            _repository.PersistAdds(existingAdds.ToImmutableHashSet());
            _repository.PersistRemoves(existingRemoves.ToImmutableHashSet());

            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var addElement = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)));
            var removeElement = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)));

            _lwwSetService.Merge(new HashSet<LWW_SetWithVCElement<TestType>> { addElement }.ToImmutableHashSet(), new HashSet<LWW_SetWithVCElement<TestType>> { removeElement }.ToImmutableHashSet());

            var lookup = _lwwSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsTrue(HashSet<LWW_SetWithVCElement<TestType>> existingAdds, HashSet<LWW_SetWithVCElement<TestType>> existingRemoves, TestType value, Node node)
        {
            _repository.PersistAdds(existingAdds.ToImmutableHashSet());
            _repository.PersistRemoves(existingRemoves.ToImmutableHashSet());

            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _lwwSetService.LocalAdd(value, new VectorClock(clock.Add(node, 0)));
            _lwwSetService.LocalRemove(value, new VectorClock(clock.Add(node, 1)));
            _lwwSetService.LocalAdd(value, new VectorClock(clock.Add(node, 2)));

            var lookup = _lwwSetService.Lookup(value);

            Assert.True(lookup);
        }

        private void AssertContains(HashSet<LWW_SetWithVCElement<TestType>> expectedValues, IEnumerable<LWW_SetWithVCElement<TestType>> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}