using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Commutative;
using CRDT.Application.Commutative.Set;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Core.Cluster;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Application.UnitTests.Commutative
{
    public class OR_SetServiceTests
    {
        private readonly IOR_SetRepository<TestType> _repository;
        private readonly OR_SetService<TestType> _orSetService;

        public OR_SetServiceTests()
        {
            _repository = new OR_SetRepository();
            _orSetService = new OR_SetService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void Add_NoExistingValues_AddsElementToTheRepository(TestType value, Guid tag)
        {
            _orSetService.Add(value, tag);

            var repositoryValues = _repository.GetAdds();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == tag);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(List<OR_SetElement<TestType>> adds, TestType value, Guid tag)
        {
            _repository.PersistAdds(adds);

            _orSetService.Add(value, tag);

            var repositoryValues = _repository.GetAdds();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == tag);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithDifferentTag_AddsElementToTheRepository(List<OR_SetElement<TestType>> adds, TestType value, Guid tag, Guid otherTag)
        {
            _repository.PersistAdds(adds);

            _orSetService.Add(value, tag);
            _orSetService.Add(value, otherTag);

            var repositoryValues = _repository.GetAdds();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Equal(2, actualValues.Count());
        }

        [Theory]
        [AutoData]
        public void Add_IsIdempotent(TestType value, Guid tag)
        {
            _orSetService.Add(value, tag);
            _orSetService.Add(value, tag);
            _orSetService.Add(value, tag);

            var repositoryValues = _repository.GetAdds();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddDoesNotExist_DoesNotAddElementToTheRepository(TestType value, Guid tag)
        {
            _orSetService.Remove(value, new[] { tag });

            var repositoryValues = _repository.GetRemoves();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value));

            Assert.Empty(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddExists_AddsElementToTheRepository(TestType value, Guid tag)
        {
            _orSetService.Add(value, tag);
            _orSetService.Remove(value, new[] { tag });

            var repositoryValues = _repository.GetRemoves();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == tag);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Remove_IsIdempotent(TestType value, Guid tag)
        {
            _orSetService.Add(value, tag);
            _orSetService.Remove(value, new[] { tag });
            _orSetService.Remove(value, new[] { tag });
            _orSetService.Remove(value, new[] { tag });

            var repositoryValues = _repository.GetRemoves();
            var actualValues = repositoryValues.Where(v => Equals(v.Value, value) && v.Tag == tag);

            Assert.Single(actualValues);
        }

        [Theory]
        [AutoData]
        public void Lookup_SingleElementAdded_ReturnsTrue(TestType value, Guid tag)
        {
            _orSetService.Add(value, tag);

            var lookup = _orSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SeveralElementsWithDifferentTags_ReturnsTrue(TestType value, Guid tag, Guid otherTag)
        {
            _orSetService.Add(value, tag);
            _orSetService.Add(value, otherTag);

            var lookup = _orSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_NonExistingElement_ReturnsFalse(TestType value)
        {
            var lookup = _orSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AllTagsRemoved_ReturnsFalse(TestType value, Guid tag, Guid otherTag)
        {
            _orSetService.Add(value, tag);
            _orSetService.Add(value, otherTag);
            _orSetService.Remove(value, new []{tag, otherTag});

            var lookup = _orSetService.Lookup(value);

            Assert.False(lookup);
        }
    }
}