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
            _lwwSetService.Merge(new List<LWW_SetWithVCElement<TestType>> { element }, new List<LWW_SetWithVCElement<TestType>>());

            var repositoryValues = _repository.GetAdds();
            Assert.Contains(element, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(List<LWW_SetWithVCElement<TestType>> adds, LWW_SetWithVCElement<TestType> element)
        {
            _repository.PersistAdds(adds);

            _lwwSetService.Merge(new List<LWW_SetWithVCElement<TestType>> { element }, new List<LWW_SetWithVCElement<TestType>>());

            var repositoryValues = _repository.GetAdds();
            Assert.Contains(element, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddDoesNotExist_DoesNotAddElementToTheRepository(LWW_SetWithVCElement<TestType> element)
        {
            _lwwSetService.Merge(new List<LWW_SetWithVCElement<TestType>>(), new List<LWW_SetWithVCElement<TestType>> { element });

            var repositoryValues = _repository.GetRemoves();
            Assert.DoesNotContain(element, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddExistsWithLowerTimestamp_AddsElementToTheRepository(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var addElement = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)));
            var removeElement = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)));

            _lwwSetService.Merge(new List<LWW_SetWithVCElement<TestType>> { addElement }, new List<LWW_SetWithVCElement<TestType>> { removeElement });

            var repositoryValues = _repository.GetRemoves();
            Assert.Contains(value, repositoryValues.Select(v => v.Value));
        }

        [Theory]
        [AutoData]
        public void Merge_NoExistingValues_AddsElementsToTheRepository(List<LWW_SetWithVCElement<TestType>> values)
        {
            _lwwSetService.Merge(values, values);

            var repositoryAdds = _repository.GetAdds();
            var repositoryRemoves = _repository.GetRemoves();

            AssertContains(values, repositoryAdds);
            AssertContains(values, repositoryRemoves);
        }

        [Theory]
        [AutoData]
        public void Merge_WithExistingValues_AddsElementsToTheRepository(List<LWW_SetWithVCElement<TestType>> existingAdds, List<LWW_SetWithVCElement<TestType>> existingRemoves, List<LWW_SetWithVCElement<TestType>> values)
        {
            _repository.PersistAdds(existingAdds);
            _repository.PersistRemoves(existingRemoves);

            _lwwSetService.Merge(values, values);

            var repositoryAdds = _repository.GetAdds();
            var repositoryRemoves = _repository.GetRemoves();

            AssertContains(values, repositoryAdds);
            AssertContains(values, repositoryRemoves);
        }

        [Theory]
        [AutoData]
        public void Merge_IsIdempotent(List<LWW_SetWithVCElement<TestType>> existingAdds, List<LWW_SetWithVCElement<TestType>> existingRemoves, List<LWW_SetWithVCElement<TestType>> values)
        {
            _repository.PersistAdds(existingAdds);
            _repository.PersistRemoves(existingRemoves);

            _lwwSetService.Merge(values, values);
            _lwwSetService.Merge(values, values);
            _lwwSetService.Merge(values, values);

            var repositoryAdds = _repository.GetAdds();
            var repositoryRemoves = _repository.GetRemoves();

            AssertContains(values, repositoryAdds);
            AssertContains(values, repositoryRemoves);
        }

        [Theory]
        [AutoData]
        public void Lookup_Added_ReturnsTrue(List<LWW_SetWithVCElement<TestType>> existingAdds, List<LWW_SetWithVCElement<TestType>> existingRemoves, LWW_SetWithVCElement<TestType> element)
        {
            _repository.PersistAdds(existingAdds);
            _repository.PersistRemoves(existingRemoves);

            _lwwSetService.Merge(new List<LWW_SetWithVCElement<TestType>> { element }, new List<LWW_SetWithVCElement<TestType>>());

            var lookup = _lwwSetService.Lookup(element.Value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_Removed_ReturnsFalse(List<LWW_SetWithVCElement<TestType>> existingAdds, List<LWW_SetWithVCElement<TestType>> existingRemoves, TestType value, Node node)
        {
            _repository.PersistAdds(existingAdds);
            _repository.PersistRemoves(existingRemoves);

            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var addElement = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)));
            var removeElement = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)));

            _lwwSetService.Merge(new List<LWW_SetWithVCElement<TestType>> { addElement }, new List<LWW_SetWithVCElement<TestType>> { removeElement });

            var lookup = _lwwSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsTrue(List<LWW_SetWithVCElement<TestType>> existingAdds, List<LWW_SetWithVCElement<TestType>> existingRemoves, TestType value, Node node)
        {
            _repository.PersistAdds(existingAdds);
            _repository.PersistRemoves(existingRemoves);

            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var addElement = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)));
            var removeElement = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)));
            var reAddElement = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 2)));

            _lwwSetService.Merge(new List<LWW_SetWithVCElement<TestType>> { addElement }, new List<LWW_SetWithVCElement<TestType>> { removeElement });

            var lookup = _lwwSetService.Lookup(value);

            Assert.False(lookup);

            _lwwSetService.Merge(new List<LWW_SetWithVCElement<TestType>> { reAddElement }, new List<LWW_SetWithVCElement<TestType>>());

            lookup = _lwwSetService.Lookup(value);

            Assert.True(lookup);
        }

        private void AssertContains(List<LWW_SetWithVCElement<TestType>> expectedValues, IEnumerable<LWW_SetWithVCElement<TestType>> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}