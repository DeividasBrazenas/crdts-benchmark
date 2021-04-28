using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Commutative;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Application.UnitTests.Commutative
{
    public class U_SetServiceTests
    {
        private readonly IU_SetRepository<TestType> _repository;
        private readonly U_SetService<TestType> _uSetService;

        public U_SetServiceTests()
        {
            _repository = new U_SetRepository();
            _uSetService = new U_SetService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void Add_NoExistingValues_AddsElementToTheRepository(TestType value)
        {
            _uSetService.Add(value);

            var repositoryValues = _repository.GetElements();
            Assert.Contains(new U_SetElement<TestType>(value, false), repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementToTheRepository(List<U_SetElement<TestType>> elements, TestType value)
        {
            _repository.PersistElements(elements);

            _uSetService.Add(value);

            var repositoryValues = _repository.GetElements();
            Assert.Contains(new U_SetElement<TestType>(value, false), repositoryValues);
        }     
        
        [Theory]
        [AutoData]
        public void Add_IsIdempotent(List<U_SetElement<TestType>> elements, TestType value)
        {
            _repository.PersistElements(elements);

            _uSetService.Add(value);
            _uSetService.Add(value);
            _uSetService.Add(value);

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(v => Equals(v, new U_SetElement<TestType>(value, false))));
        }

        [Theory]
        [AutoData]
        public void Remove_AddDoesNotExist_DoesNotAddElementToTheRepository(TestType value)
        {
            _uSetService.Remove(value);

            var repositoryValues = _repository.GetElements();
            Assert.DoesNotContain(new U_SetElement<TestType>(value, true), repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Remove_AddExists_AddsElementToTheRepository(TestType value)
        {
            _uSetService.Add(value);
            _uSetService.Remove(value);

            var repositoryValues = _repository.GetElements();
            Assert.Contains(new U_SetElement<TestType>(value, true), repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Remove_IsIdempotent(TestType value)
        {
            _uSetService.Add(value);
            _uSetService.Remove(value);
            _uSetService.Remove(value);
            _uSetService.Remove(value);

            var repositoryValues = _repository.GetElements();
            Assert.Equal(1, repositoryValues.Count(v => Equals(v, new U_SetElement<TestType>(value, true))));
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsTrue(TestType value)
        {
            _uSetService.Add(value);

            var lookup = _uSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsFalse(TestType value)
        {
            _uSetService.Add(value);
            _uSetService.Remove(value);

            var lookup = _uSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdd_ReturnsFalse(TestType value)
        {
            _uSetService.Add(value);
            _uSetService.Remove(value);
            _uSetService.Add(value);

            var lookup = _uSetService.Lookup(value);

            Assert.False(lookup);
        }
    }
}