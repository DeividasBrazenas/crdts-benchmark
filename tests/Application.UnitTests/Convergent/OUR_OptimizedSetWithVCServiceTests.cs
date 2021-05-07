using System;
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
using static CRDT.UnitTestHelpers.TestTypes.TestTypeBuilder;

namespace CRDT.Application.UnitTests.Convergent
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
        public void MergeAdds_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var element = new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)), false);
            _ourSetService.Merge(new HashSet<OUR_OptimizedSetWithVCElement<TestType>> { element }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(HashSet<OUR_OptimizedSetWithVCElement<TestType>> values)
        {
            _ourSetService.Merge(values.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_UpdatedElementWithHigherTimestamp_ReplacesElementsInRepository(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var element = new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)), false);

            _repository.PersistElements(new HashSet<OUR_OptimizedSetWithVCElement<TestType>> { element }.ToImmutableHashSet());

            var newElement = new OUR_OptimizedSetWithVCElement<TestType>(_builder.Build(value.Id), tag, new VectorClock(clock.Add(node, 1)), false);

            _ourSetService.LocalAdd(newElement.Value, newElement.Tag, newElement.VectorClock);

            var repositoryValues = _repository.GetElements();
            Assert.Equal(0, repositoryValues.Count(x => Equals(x, element)));
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, newElement)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_UpdatedElementWithLowerTimestamp_DoesNotDoAnything(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var element = new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 1)), false);

            _repository.PersistElements(new HashSet<OUR_OptimizedSetWithVCElement<TestType>> { element }.ToImmutableHashSet());

            var newElement = new OUR_OptimizedSetWithVCElement<TestType>(_builder.Build(value.Id), tag, new VectorClock(clock.Add(node, 0)), false);

            _ourSetService.LocalAdd(newElement.Value, newElement.Tag, newElement.VectorClock);

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
            Assert.Equal(0, repositoryValues.Count(x => Equals(x, newElement)));
        }
        [Theory]
        [AutoData]
        public void MergeAdds_SingleElement_AddsElementsToTheRepository(HashSet<OUR_OptimizedSetWithVCElement<TestType>> existingValues, TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistElements(existingValues.ToImmutableHashSet());

            var element = new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)), false);
            _ourSetService.Merge(new HashSet<OUR_OptimizedSetWithVCElement<TestType>> { element }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElements_AddsElementsToTheRepository(HashSet<OUR_OptimizedSetWithVCElement<TestType>> existingValues, HashSet<OUR_OptimizedSetWithVCElement<TestType>> values)
        {
            _repository.PersistElements(existingValues.ToImmutableHashSet());

            _ourSetService.Merge(values.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsIdempotent(HashSet<OUR_OptimizedSetWithVCElement<TestType>> values, TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistElements(values.ToImmutableHashSet());

            var element = new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)), false);

            values.Add(element);

            _ourSetService.Merge(values.ToImmutableHashSet());
            _ourSetService.Merge(values.ToImmutableHashSet());
            _ourSetService.Merge(values.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsCommutative(Guid firstTag, Guid secondTag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var firstElement = new OUR_OptimizedSetWithVCElement<TestType>(_builder.Build(), firstTag, new VectorClock(clock.Add(node, 0)), false);
            var secondElement = new OUR_OptimizedSetWithVCElement<TestType>(_builder.Build(), secondTag, new VectorClock(clock.Add(node, 0)), false);
            var thirdElement = new OUR_OptimizedSetWithVCElement<TestType>(_builder.Build(), secondTag, new VectorClock(clock.Add(node, 0)), false);
            var fourthElement = new OUR_OptimizedSetWithVCElement<TestType>(_builder.Build(), firstTag, new VectorClock(clock.Add(node, 0)), false);
            var fifthElement = new OUR_OptimizedSetWithVCElement<TestType>(_builder.Build(), firstTag, new VectorClock(clock.Add(node, 0)), false);

            var firstRepository = new OUR_OptimizedSetWithVCRepository();
            var firstService = new OUR_OptimizedSetWithVCService<TestType>(firstRepository);

            _repository.PersistElements(new HashSet<OUR_OptimizedSetWithVCElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet());
            firstService.Merge(new HashSet<OUR_OptimizedSetWithVCElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet());

            var firstRepositoryValues = firstRepository.GetElements();

            var secondRepository = new OUR_OptimizedSetWithVCRepository();
            var secondService = new OUR_OptimizedSetWithVCService<TestType>(secondRepository);

            _repository.PersistElements(new HashSet<OUR_OptimizedSetWithVCElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet());
            secondService.Merge(new HashSet<OUR_OptimizedSetWithVCElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet());

            var secondRepositoryValues = firstRepository.GetElements();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var element = new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 1)), true);

            _repository.PersistElements(new HashSet<OUR_OptimizedSetWithVCElement<TestType>> { new(value, tag, new VectorClock(clock.Add(node, 0)), false) }.ToImmutableHashSet());
            _ourSetService.Merge(new HashSet<OUR_OptimizedSetWithVCElement<TestType>> { element }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SeveralElements_AddsElementsToTheRepository(HashSet<OUR_OptimizedSetWithVCElement<TestType>> existingValues, TestType one, TestType two, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistElements(existingValues.ToImmutableHashSet());

            _ourSetService.Merge(new HashSet<OUR_OptimizedSetWithVCElement<TestType>>
            {
                new (one, tag, new VectorClock(clock.Add(node, 0)), true),
                new (two, tag, new VectorClock(clock.Add(node, 0)), true),
            }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, new OUR_OptimizedSetWithVCElement<TestType>(one, tag, new VectorClock(clock.Add(node, 0)), true))));
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, new OUR_OptimizedSetWithVCElement<TestType>(two, tag, new VectorClock(clock.Add(node, 0)), true))));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_IsIdempotent(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var element = new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)), true);

            _ourSetService.Merge(new HashSet<OUR_OptimizedSetWithVCElement<TestType>> { element }.ToImmutableHashSet());
            _ourSetService.Merge(new HashSet<OUR_OptimizedSetWithVCElement<TestType>> { element }.ToImmutableHashSet());
            _ourSetService.Merge(new HashSet<OUR_OptimizedSetWithVCElement<TestType>> { element }.ToImmutableHashSet());

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void Lookup_SingleElementAdded_ReturnsTrue(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.Merge(new HashSet<OUR_OptimizedSetWithVCElement<TestType>> { new(value, tag, new VectorClock(clock.Add(node, 0)), false) }.ToImmutableHashSet());

            var lookup = _ourSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SeveralElementsWithDifferentTags_ReturnsTrue(TestType value, Guid firstTag, Guid secondTag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.Merge(new HashSet<OUR_OptimizedSetWithVCElement<TestType>>
            {
                new(value, firstTag, new VectorClock(clock.Add(node, 0)), false),
                new(value, secondTag, new VectorClock(clock.Add(node, 0)), false)
            }.ToImmutableHashSet());

            var lookup = _ourSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_NonExistingElement_ReturnsFalse(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.Merge(new HashSet<OUR_OptimizedSetWithVCElement<TestType>>
            {
                new(value, tag, new VectorClock(clock.Add(node, 0)), true)
            }.ToImmutableHashSet());

            var lookup = _ourSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AllTagsRemoved_ReturnsFalse(TestType value, Guid firstTag, Guid secondTag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _ourSetService.Merge(new HashSet<OUR_OptimizedSetWithVCElement<TestType>>
            {
                new(value, firstTag, new VectorClock(clock.Add(node, 0)), true),
                new(value, secondTag, new VectorClock(clock.Add(node, 0)), true)
            }.ToImmutableHashSet());

            var lookup = _ourSetService.Lookup(value);

            Assert.False(lookup);
        }

        private void AssertContains(HashSet<OUR_OptimizedSetWithVCElement<TestType>> expectedValues, IEnumerable<OUR_OptimizedSetWithVCElement<TestType>> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}