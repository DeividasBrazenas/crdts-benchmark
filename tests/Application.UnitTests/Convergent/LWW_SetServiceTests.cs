using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Convergent.Set;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Application.UnitTests.Convergent
{
    public class LWW_SetServiceTests
    {
        private readonly ILWW_SetRepository<TestType> _repository;
        private readonly LWW_SetService<TestType> _lwwSetService;

        public LWW_SetServiceTests()
        {
            _repository = new LWW_SetRepository();
            _lwwSetService = new LWW_SetService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void Add_NoExistingValues_AddsElementToTheRepository(LWW_SetElement<TestType> element)
        {
            _lwwSetService.Merge(new HashSet<LWW_SetElement<TestType>> { element }.ToImmutableHashSet(), ImmutableHashSet<LWW_SetElement<TestType>>.Empty);

            var repositoryValues = _repository.GetAdds();
            Assert.Contains(element, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(HashSet<LWW_SetElement<TestType>> adds, LWW_SetElement<TestType> element)
        {
            _repository.PersistAdds(adds.ToImmutableHashSet());

            _lwwSetService.Merge(new HashSet<LWW_SetElement<TestType>> { element }.ToImmutableHashSet(), ImmutableHashSet<LWW_SetElement<TestType>>.Empty);

            var repositoryValues = _repository.GetAdds();
            Assert.Contains(element, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddExistsWithLowerTimestamp_AddsElementToTheRepository(TestType value, long timestamp)
        {
            var addElement = new LWW_SetElement<TestType>(value, timestamp);
            var removeElement = new LWW_SetElement<TestType>(value, timestamp + 10);

            _lwwSetService.Merge(new HashSet<LWW_SetElement<TestType>> { addElement }.ToImmutableHashSet(), new HashSet<LWW_SetElement<TestType>> { removeElement }.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            Assert.Contains(value, repositoryValues.Select(v => v.Value));
        }

        [Theory]
        [AutoData]
        public void Merge_NoExistingValues_AddsElementsToTheRepository(HashSet<LWW_SetElement<TestType>> values)
        {
            _lwwSetService.Merge(values.ToImmutableHashSet(), values.ToImmutableHashSet());

            var repositoryAdds = _repository.GetAdds();
            var repositoryRemoves = _repository.GetRemoves();

            AssertContains(values, repositoryAdds);
            AssertContains(values, repositoryRemoves);
        }

        [Theory]
        [AutoData]
        public void Merge_WithExistingValues_AddsElementsToTheRepository(HashSet<LWW_SetElement<TestType>> existingAdds, HashSet<LWW_SetElement<TestType>> existingRemoves, HashSet<LWW_SetElement<TestType>> values)
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
        public void Merge_IsIdempotent(HashSet<LWW_SetElement<TestType>> existingAdds, HashSet<LWW_SetElement<TestType>> existingRemoves, HashSet<LWW_SetElement<TestType>> values)
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
        public void Lookup_Added_ReturnsTrue(HashSet<LWW_SetElement<TestType>> existingAdds, HashSet<LWW_SetElement<TestType>> existingRemoves, LWW_SetElement<TestType> element)
        {
            _repository.PersistAdds(existingAdds.ToImmutableHashSet());
            _repository.PersistRemoves(existingRemoves.ToImmutableHashSet());

            _lwwSetService.Merge(new HashSet<LWW_SetElement<TestType>> { element }.ToImmutableHashSet(), ImmutableHashSet<LWW_SetElement<TestType>>.Empty);

            var lookup = _lwwSetService.Lookup(element.Value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_Removed_ReturnsFalse(HashSet<LWW_SetElement<TestType>> existingAdds, HashSet<LWW_SetElement<TestType>> existingRemoves, TestType value, long timestamp)
        {
            _repository.PersistAdds(existingAdds.ToImmutableHashSet());
            _repository.PersistRemoves(existingRemoves.ToImmutableHashSet());

            var addElement = new LWW_SetElement<TestType>(value, timestamp);
            var removeElement = new LWW_SetElement<TestType>(value, timestamp + 100);

            _lwwSetService.Merge(new HashSet<LWW_SetElement<TestType>> { addElement }.ToImmutableHashSet(), new HashSet<LWW_SetElement<TestType>> { removeElement }.ToImmutableHashSet());

            var lookup = _lwwSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsTrue(HashSet<LWW_SetElement<TestType>> existingAdds, HashSet<LWW_SetElement<TestType>> existingRemoves, TestType value, long timestamp)
        {
            _repository.PersistAdds(existingAdds.ToImmutableHashSet());
            _repository.PersistRemoves(existingRemoves.ToImmutableHashSet());

            _lwwSetService.LocalAdd(value, timestamp);
            _lwwSetService.LocalRemove(value, timestamp + 100);
            _lwwSetService.LocalAdd(value, timestamp + 200);

            var lookup = _lwwSetService.Lookup(value);

            Assert.True(lookup);
        }

        private void AssertContains(HashSet<LWW_SetElement<TestType>> expectedValues, IEnumerable<LWW_SetElement<TestType>> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}