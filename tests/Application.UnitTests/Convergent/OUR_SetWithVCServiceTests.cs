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
    public class OUR_SetWithVCServiceTests
    {
        private readonly IOUR_SetWithVCRepository<TestType> _repository;
        private readonly OUR_SetWithVCService<TestType> _orSetService;
        private readonly TestTypeBuilder _builder;
        public OUR_SetWithVCServiceTests()
        {
            _repository = new OUR_SetWithVCRepository();
            _orSetService = new OUR_SetWithVCService<TestType>(_repository);
            _builder = new TestTypeBuilder(new Random());
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var element = new OUR_SetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)));
            _orSetService.Merge(new List<OUR_SetWithVCElement<TestType>> { element }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(List<OUR_SetWithVCElement<TestType>> values)
        {
            _orSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_UpdatedElementWithHigherTimestamp_ReplacesElementsInRepository(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var element = new OUR_SetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)));

            _repository.PersistAdds(new List<OUR_SetWithVCElement<TestType>> { element }.ToImmutableHashSet());

            var newElement = new OUR_SetWithVCElement<TestType>(_builder.Build(value.Id), tag, new VectorClock(clock.Add(node, 1)));

            _orSetService.LocalAdd(newElement.Value, newElement.Tag, newElement.VectorClock);

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(0, repositoryValues.Count(x => Equals(x, element)));
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, newElement)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_UpdatedElementWithLowerTimestamp_DoesNotDoAnything(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var element = new OUR_SetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 1)));

            _repository.PersistAdds(new List<OUR_SetWithVCElement<TestType>> { element }.ToImmutableHashSet());

            var newElement = new OUR_SetWithVCElement<TestType>(_builder.Build(value.Id), tag, new VectorClock(clock.Add(node, 0)));

            _orSetService.LocalAdd(newElement.Value, newElement.Tag, newElement.VectorClock);

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
            Assert.Equal(0, repositoryValues.Count(x => Equals(x, newElement)));
        }
        [Theory]
        [AutoData]
        public void MergeAdds_SingleElement_AddsElementsToTheRepository(List<OUR_SetWithVCElement<TestType>> existingValues, TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistAdds(existingValues.ToImmutableHashSet());

            var element = new OUR_SetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)));
            _orSetService.Merge(new List<OUR_SetWithVCElement<TestType>> { element }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, element)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElements_AddsElementsToTheRepository(List<OUR_SetWithVCElement<TestType>> existingValues, List<OUR_SetWithVCElement<TestType>> values)
        {
            _repository.PersistAdds(existingValues.ToImmutableHashSet());

            _orSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsIdempotent(List<OUR_SetWithVCElement<TestType>> values, TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            _repository.PersistAdds(values.ToImmutableHashSet());

            var element = new OUR_SetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)));

            values.Add(element);

            _orSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);
            _orSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);
            _orSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsCommutative(Guid firstTag, Guid secondTag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var firstElement = new OUR_SetWithVCElement<TestType>(_builder.Build(), firstTag, new VectorClock(clock.Add(node, 0)));
            var secondElement = new OUR_SetWithVCElement<TestType>(_builder.Build(), secondTag, new VectorClock(clock.Add(node, 0)));
            var thirdElement = new OUR_SetWithVCElement<TestType>(_builder.Build(), secondTag, new VectorClock(clock.Add(node, 0)));
            var fourthElement = new OUR_SetWithVCElement<TestType>(_builder.Build(), firstTag, new VectorClock(clock.Add(node, 0)));
            var fifthElement = new OUR_SetWithVCElement<TestType>(_builder.Build(), firstTag, new VectorClock(clock.Add(node, 0)));

            var firstRepository = new OUR_SetWithVCRepository();
            var firstService = new OUR_SetWithVCService<TestType>(firstRepository);

            _repository.PersistAdds(new List<OUR_SetWithVCElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet());
            firstService.Merge(new List<OUR_SetWithVCElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);

            var firstRepositoryValues = firstRepository.GetAdds();

            var secondRepository = new OUR_SetWithVCRepository();
            var secondService = new OUR_SetWithVCService<TestType>(secondRepository);

            _repository.PersistAdds(new List<OUR_SetWithVCElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet());
            secondService.Merge(new List<OUR_SetWithVCElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);

            var secondRepositoryValues = firstRepository.GetAdds();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleValueWithEmptyRepository_AddsElementsToTheRepository(OUR_SetWithVCElement<TestType> value)
        {
            _repository.PersistAdds(new List<OUR_SetWithVCElement<TestType>> { value }.ToImmutableHashSet());
            _orSetService.Merge(ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty, new List<OUR_SetWithVCElement<TestType>> { value }.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(List<OUR_SetWithVCElement<TestType>> values)
        {
            _repository.PersistAdds(values.ToImmutableHashSet());
            _orSetService.Merge(ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty, values.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleElement_AddsElementsToTheRepository(List<OUR_SetWithVCElement<TestType>> existingValues, OUR_SetWithVCElement<TestType> value)
        {
            _repository.PersistRemoves(existingValues.ToImmutableHashSet());
            _repository.PersistAdds(new List<OUR_SetWithVCElement<TestType>> { value }.ToImmutableHashSet());

            _orSetService.Merge(ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty, new List<OUR_SetWithVCElement<TestType>> { value }.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SeveralElements_AddsElementsToTheRepository(List<OUR_SetWithVCElement<TestType>> existingValues, List<OUR_SetWithVCElement<TestType>> values)
        {
            _repository.PersistRemoves(existingValues.ToImmutableHashSet());
            _repository.PersistAdds(values.ToImmutableHashSet());

            _orSetService.Merge(ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty, values.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_IsIdempotent(List<OUR_SetWithVCElement<TestType>> values, OUR_SetWithVCElement<TestType> value)
        {
            _repository.PersistRemoves(values.ToImmutableHashSet());
            _repository.PersistAdds(new List<OUR_SetWithVCElement<TestType>> { value }.ToImmutableHashSet());

            values.Add(value);

            _orSetService.Merge(ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty, values.ToImmutableHashSet());
            _orSetService.Merge(ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty, values.ToImmutableHashSet());
            _orSetService.Merge(ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty, values.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_IsCommutative(Guid firstTag, Guid secondTag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var firstElement = new OUR_SetWithVCElement<TestType>(_builder.Build(), firstTag, new VectorClock(clock.Add(node, 0)));
            var secondElement = new OUR_SetWithVCElement<TestType>(_builder.Build(), secondTag, new VectorClock(clock.Add(node, 0)));
            var thirdElement = new OUR_SetWithVCElement<TestType>(_builder.Build(), secondTag, new VectorClock(clock.Add(node, 0)));
            var fourthElement = new OUR_SetWithVCElement<TestType>(_builder.Build(), firstTag, new VectorClock(clock.Add(node, 0)));
            var fifthElement = new OUR_SetWithVCElement<TestType>(_builder.Build(), firstTag, new VectorClock(clock.Add(node, 0)));

            var firstRepository = new OUR_SetWithVCRepository();
            var firstService = new OUR_SetWithVCService<TestType>(firstRepository);

            firstRepository.PersistRemoves(new List<OUR_SetWithVCElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet());
            firstRepository.PersistAdds(new List<OUR_SetWithVCElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet());
            firstService.Merge(ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty, new List<OUR_SetWithVCElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet());

            var firstRepositoryValues = firstRepository.GetRemoves();

            var secondRepository = new OUR_SetWithVCRepository();
            var secondService = new OUR_SetWithVCService<TestType>(secondRepository);

            secondRepository.PersistRemoves(new List<OUR_SetWithVCElement<TestType>> { fourthElement, fifthElement }.ToImmutableHashSet());
            secondRepository.PersistAdds(new List<OUR_SetWithVCElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet());
            secondService.Merge(ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty, new List<OUR_SetWithVCElement<TestType>> { firstElement, secondElement, thirdElement }.ToImmutableHashSet());

            var secondRepositoryValues = firstRepository.GetRemoves();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void Lookup_SingleElementAdded_ReturnsTrue(OUR_SetWithVCElement<TestType> value)
        {
            _orSetService.Merge(new List<OUR_SetWithVCElement<TestType>> { value }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);

            var lookup = _orSetService.Lookup(value.Value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SeveralElementsWithDifferentTags_ReturnsTrue(TestType value, Guid firstTag, Guid secondTag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var elements = new List<OUR_SetWithVCElement<TestType>>
            {
                new (value, firstTag, new VectorClock(clock.Add(node, 0))),
                new (value, secondTag, new VectorClock(clock.Add(node, 0)))
            };
            _orSetService.Merge(elements.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);

            var lookup = _orSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_NonExistingElement_ReturnsFalse(OUR_SetWithVCElement<TestType> value)
        {
            _orSetService.Merge(new List<OUR_SetWithVCElement<TestType>> { value }.ToImmutableHashSet(), new List<OUR_SetWithVCElement<TestType>> { value }.ToImmutableHashSet());

            var lookup = _orSetService.Lookup(value.Value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AllTagsRemoved_ReturnsFalse(TestType value, Guid firstTag, Guid secondTag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var elements = new List<OUR_SetWithVCElement<TestType>>
            {
                new (value, firstTag, new VectorClock(clock.Add(node, 0))),
                new (value, secondTag, new VectorClock(clock.Add(node, 0)))
            };
            _orSetService.Merge(elements.ToImmutableHashSet(), elements.ToImmutableHashSet());

            var lookup = _orSetService.Lookup(value);

            Assert.False(lookup);
        }

        private void AssertContains(List<OUR_SetWithVCElement<TestType>> expectedValues, IEnumerable<OUR_SetWithVCElement<TestType>> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}