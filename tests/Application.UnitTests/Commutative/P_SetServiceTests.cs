using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Commutative.Set;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Application.UnitTests.Commutative
{
    public class P_SetServiceTests
    {
        private readonly IP_SetRepository<TestType> _repository;
        private readonly P_SetService<TestType> _pSetService;

        public P_SetServiceTests()
        {
            _repository = new P_SetRepository();
            _pSetService = new P_SetService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void Add_NoExistingValues_AddsElementToTheRepository(TestType value)
        {
            _pSetService.DownstreamAdd(value);

            var repositoryValues = _repository.GetAdds();
            Assert.Contains(value, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(HashSet<TestType> adds, TestType value)
        {
            _repository.PersistAdds(adds.ToImmutableHashSet());

            _pSetService.DownstreamAdd(value);

            var repositoryValues = _repository.GetAdds();
            Assert.Contains(value, repositoryValues);
        }     
        
        [Theory]
        [AutoData]
        public void Add_IsIdempotent(HashSet<TestType> adds, TestType value)
        {
            _repository.PersistAdds(adds.ToImmutableHashSet());

            _pSetService.DownstreamAdd(value);
            _pSetService.DownstreamAdd(value);
            _pSetService.DownstreamAdd(value);

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(1, repositoryValues.Count(v => Equals(v, value)));
        }

        [Theory]
        [AutoData]
        public void Remove_AddDoesNotExist_DoesNotAddElementToTheRepository(TestType value)
        {
            _pSetService.DownstreamRemove(value);

            var repositoryValues = _repository.GetRemoves();
            Assert.DoesNotContain(value, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddExists_AddsElementToTheRepository(TestType value)
        {
            _pSetService.DownstreamAdd(value);
            _pSetService.DownstreamRemove(value);

            var repositoryValues = _repository.GetRemoves();
            Assert.Contains(value, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Remove_IsIdempotent(TestType value)
        {
            _pSetService.DownstreamAdd(value);
            _pSetService.DownstreamRemove(value);
            _pSetService.DownstreamRemove(value);
            _pSetService.DownstreamRemove(value);

            var repositoryValues = _repository.GetRemoves();
            Assert.Equal(1, repositoryValues.Count(v => Equals(v, value)));
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsTrue(HashSet<TestType> existingAdds, HashSet<TestType> existingRemoves, TestType value)
        {
            _repository.PersistAdds(existingAdds.ToImmutableHashSet());
            _repository.PersistRemoves(existingRemoves.ToImmutableHashSet());

            _pSetService.DownstreamAdd(value);

            var lookup = _pSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsFalse(HashSet<TestType> existingAdds, HashSet<TestType> existingRemoves, TestType value)
        {
            _repository.PersistAdds(existingAdds.ToImmutableHashSet());
            _repository.PersistRemoves(existingRemoves.ToImmutableHashSet());

            _pSetService.DownstreamAdd(value);
            _pSetService.DownstreamRemove(value);

            var lookup = _pSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdd_ReturnsFalse(HashSet<TestType> existingAdds, HashSet<TestType> existingRemoves, TestType value)
        {
            _repository.PersistAdds(existingAdds.ToImmutableHashSet());
            _repository.PersistRemoves(existingRemoves.ToImmutableHashSet());

            _pSetService.DownstreamAdd(value);
            _pSetService.DownstreamRemove(value);
            _pSetService.DownstreamAdd(value);

            var lookup = _pSetService.Lookup(value);

            Assert.False(lookup);
        }
    }
}