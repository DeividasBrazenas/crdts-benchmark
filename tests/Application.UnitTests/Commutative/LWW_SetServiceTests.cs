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
        public void Add_NoExistingValues_AddsElementToTheRepository(TestType value, long timestamp)
        {
            _lwwSetService.DownstreamAssign(value, timestamp);

            var repositoryValues = _repository.GetAdds();
            Assert.Contains(value, repositoryValues.Select(v => v.Value));
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(HashSet<LWW_SetElement<TestType>> adds, TestType value, long timestamp)
        {
            _repository.PersistAdds(adds.ToImmutableHashSet());

            _lwwSetService.DownstreamAssign(value, timestamp);

            var repositoryValues = _repository.GetAdds();
            Assert.Contains(value, repositoryValues.Select(v => v.Value));
        }

        [Theory]
        [AutoData]
        public void Remove_AddDoesNotExist_DoesNotAddElementToTheRepository(TestType value, long timestamp)
        {
            _lwwSetService.DownstreamRemove(value, timestamp);

            var repositoryValues = _repository.GetRemoves();
            Assert.DoesNotContain(value, repositoryValues.Select(v => v.Value));
        }

        [Theory]
        [AutoData]
        public void Remove_AddExistsWithLowerTimestamp_AddsElementToTheRepository(TestType value, long timestamp)
        {
            _lwwSetService.DownstreamAssign(value, timestamp);
            _lwwSetService.DownstreamRemove(value, timestamp + 10);

            var repositoryValues = _repository.GetRemoves();
            Assert.Contains(value, repositoryValues.Select(v => v.Value));
        }

        [Theory]
        [AutoData]
        public void Lookup_Added_ReturnsTrue(HashSet<LWW_SetElement<TestType>> existingAdds, HashSet<LWW_SetElement<TestType>> existingRemoves, TestType value, long timestamp)
        {
            _repository.PersistAdds(existingAdds.ToImmutableHashSet());
            _repository.PersistRemoves(existingRemoves.ToImmutableHashSet());

            _lwwSetService.DownstreamAssign(value, timestamp);

            var lookup = _lwwSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_Removed_ReturnsFalse(HashSet<LWW_SetElement<TestType>> existingAdds, HashSet<LWW_SetElement<TestType>> existingRemoves, TestType value, long timestamp)
        {
            _repository.PersistAdds(existingAdds.ToImmutableHashSet());
            _repository.PersistRemoves(existingRemoves.ToImmutableHashSet());

            _lwwSetService.DownstreamAssign(value, timestamp);
            _lwwSetService.DownstreamRemove(value, timestamp + 100);

            var lookup = _lwwSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsTrue(HashSet<LWW_SetElement<TestType>> existingAdds, HashSet<LWW_SetElement<TestType>> existingRemoves, TestType value, long timestamp)
        {
            _repository.PersistAdds(existingAdds.ToImmutableHashSet());
            _repository.PersistRemoves(existingRemoves.ToImmutableHashSet());

            _lwwSetService.DownstreamAssign(value, timestamp);
            _lwwSetService.DownstreamRemove(value, timestamp + 100);
            _lwwSetService.DownstreamAssign(value, timestamp + 200);

            var lookup = _lwwSetService.Lookup(value);

            Assert.True(lookup);
        }
    }
}