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
    public class OUR_SetWithVCServiceTests
    {
        private readonly IOUR_SetWithVCRepository<TestType> _repository;
        private readonly OUR_SetWithVCService<TestType> _ourSetService;

        public OUR_SetWithVCServiceTests()
        {
            _repository = new OUR_SetWithVCRepository();
            _ourSetService = new OUR_SetWithVCService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void Add_NoExistingValues_AddsElementToTheRepository(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.Add(value, tag, new VectorClock(clock.Add(node, 0)));

            var repositoryValues = _repository.GetAdds();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == tag && v.VectorClock.Equals(new VectorClock(clock.Add(node, 0))));

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(List<OUR_SetWithVCElement<TestType>> adds, TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistAdds(adds);

            _ourSetService.Add(value, tag, new VectorClock(clock.Add(node, 0)));

            var repositoryValues = _repository.GetAdds();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == tag && v.VectorClock.Equals(new VectorClock(clock.Add(node, 0))));

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithDifferentTag_AddsElementToTheRepository(List<OUR_SetWithVCElement<TestType>> adds, TestType value, Guid tag, Guid otherTag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistAdds(adds);

            _ourSetService.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            _ourSetService.Add(value, otherTag, new VectorClock(clock.Add(node, 0)));

            var repositoryValues = _repository.GetAdds();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Equal(2, actualValues.Count());
        }

        [Theory]
        [AutoData]
        public void Add_IsIdempotent(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            _ourSetService.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            _ourSetService.Add(value, tag, new VectorClock(clock.Add(node, 0)));

            var repositoryValues = _repository.GetAdds();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddDoesNotExist_DoesNotAddElementToTheRepository(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.Remove(value, new[] { tag }, new VectorClock(clock.Add(node, 0)));

            var repositoryValues = _repository.GetRemoves();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Empty(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddExists_AddsElementToTheRepository(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            _ourSetService.Remove(value, new[] { tag }, new VectorClock(clock.Add(node, 0)));

            var repositoryValues = _repository.GetRemoves();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == tag);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_IsIdempotent(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            _ourSetService.Remove(value, new[] { tag }, new VectorClock(clock.Add(node, 0)));
            _ourSetService.Remove(value, new[] { tag }, new VectorClock(clock.Add(node, 0)));
            _ourSetService.Remove(value, new[] { tag }, new VectorClock(clock.Add(node, 0)));

            var repositoryValues = _repository.GetRemoves();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == tag);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Lookup_SingleElementAdded_ReturnsTrue(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.Add(value, tag, new VectorClock(clock.Add(node, 0)));

            var lookup = _ourSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SeveralElementsWithDifferentTags_ReturnsTrue(TestType value, Guid tag, Guid otherTag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            _ourSetService.Add(value, otherTag, new VectorClock(clock.Add(node, 5)));

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

            _ourSetService.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            _ourSetService.Add(value, otherTag, new VectorClock(clock.Add(node, 0)));
            _ourSetService.Remove(value, new[] { tag, otherTag }, new VectorClock(clock.Add(node, 0)));

            var lookup = _ourSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_UpdatedElement_ReturnsTrueForUpdatedElement(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.Add(value, tag, new VectorClock(clock.Add(node, 0)));

            var newValue = Build(value.Id);
            _ourSetService.Update(newValue, tag, new VectorClock(clock.Add(node, 3)));

            var lookup = _ourSetService.Lookup(newValue);
            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_UpdatedElement_ReturnsFalseForOldElement(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.Add(value, tag, new VectorClock(clock.Add(node, 0)));

            var newValue = Build(value.Id);
            _ourSetService.Update(newValue, tag, new VectorClock(clock.Add(node, 3)));

            var lookup = _ourSetService.Lookup(value);
            Assert.False(lookup);
        }
    }
}