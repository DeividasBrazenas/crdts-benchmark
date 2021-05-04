using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using AutoFixture.Xunit2;
using CRDT.Application.Convergent.Counter;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Counters.Entities;
using Xunit;

namespace CRDT.Application.UnitTests.Convergent
{
    public class PN_CounterServiceTests
    {
        private readonly PN_CounterRepository _repository;
        private readonly PN_CounterService _service;

        public PN_CounterServiceTests()
        {
            _repository = new PN_CounterRepository();
            _service = new PN_CounterService(_repository);
        }

        [Theory]
        [AutoData]
        public void Merge_TakesMaxValues(Guid nodeOneId, Guid nodeTwoId, Guid nodeThreeId, Guid nodeFourId)
        {
            var additions = new List<CounterElement> { new(7, nodeOneId), new(17, nodeTwoId), new(9, nodeThreeId) };
            var otherAdditions = new List<CounterElement> { new(3, nodeTwoId), new(42, nodeThreeId), new(10, nodeFourId) };

            var subtractions = new List<CounterElement> { new(2, nodeOneId), new(9, nodeTwoId), new(5, nodeThreeId) };
            var otherSubtractions = new List<CounterElement> { new(3, nodeTwoId), new(11, nodeThreeId), new(7, nodeFourId) };

            _repository.PersistAdditions(additions);
            _repository.PersistSubtractions(subtractions);

            _service.Merge(otherAdditions.ToImmutableHashSet(), otherSubtractions.ToImmutableHashSet());

            Assert.Equal(4, _repository.Additions.Count);
            Assert.Contains(_repository.Additions, e => e.Value == 7 && e.Node.Id == nodeOneId);
            Assert.Contains(_repository.Additions, e => e.Value == 17 && e.Node.Id == nodeTwoId);
            Assert.Contains(_repository.Additions, e => e.Value == 42 && e.Node.Id == nodeThreeId);
            Assert.Contains(_repository.Additions, e => e.Value == 10 && e.Node.Id == nodeFourId);

            Assert.Equal(4, _repository.Subtractions.Count);
            Assert.Contains(_repository.Subtractions, e => e.Value == 2 && e.Node.Id == nodeOneId);
            Assert.Contains(_repository.Subtractions, e => e.Value == 9 && e.Node.Id == nodeTwoId);
            Assert.Contains(_repository.Subtractions, e => e.Value == 11 && e.Node.Id == nodeThreeId);
            Assert.Contains(_repository.Subtractions, e => e.Value == 7 && e.Node.Id == nodeFourId);
        }

        [Theory]
        [AutoData]
        public void Merge_IsCommutative(Guid nodeOneId, Guid nodeTwoId, Guid nodeThreeId, Guid nodeFourId)
        {
            var additions = new List<CounterElement> { new(7, nodeOneId), new(17, nodeTwoId), new(9, nodeThreeId) };
            var otherAdditions = new List<CounterElement> { new(3, nodeTwoId), new(42, nodeThreeId), new(10, nodeFourId) };

            var subtractions = new List<CounterElement> { new(2, nodeOneId), new(9, nodeTwoId), new(5, nodeThreeId) };
            var otherSubtractions = new List<CounterElement> { new(3, nodeTwoId), new(11, nodeThreeId), new(7, nodeFourId) };

            _repository.PersistAdditions(additions);
            _repository.PersistSubtractions(subtractions);

            _service.Merge(otherAdditions.ToImmutableHashSet(), otherSubtractions.ToImmutableHashSet());
            _service.Merge(otherAdditions.ToImmutableHashSet(), otherSubtractions.ToImmutableHashSet());
            _service.Merge(otherAdditions.ToImmutableHashSet(), otherSubtractions.ToImmutableHashSet());
            _service.Merge(otherAdditions.ToImmutableHashSet(), otherSubtractions.ToImmutableHashSet());

            Assert.Equal(4, _repository.Additions.Count);
            Assert.Contains(_repository.Additions, e => e.Value == 7 && e.Node.Id == nodeOneId);
            Assert.Contains(_repository.Additions, e => e.Value == 17 && e.Node.Id == nodeTwoId);
            Assert.Contains(_repository.Additions, e => e.Value == 42 && e.Node.Id == nodeThreeId);
            Assert.Contains(_repository.Additions, e => e.Value == 10 && e.Node.Id == nodeFourId);

            Assert.Equal(4, _repository.Subtractions.Count);
            Assert.Contains(_repository.Subtractions, e => e.Value == 2 && e.Node.Id == nodeOneId);
            Assert.Contains(_repository.Subtractions, e => e.Value == 9 && e.Node.Id == nodeTwoId);
            Assert.Contains(_repository.Subtractions, e => e.Value == 11 && e.Node.Id == nodeThreeId);
            Assert.Contains(_repository.Subtractions, e => e.Value == 7 && e.Node.Id == nodeFourId);
        }

        [Theory]
        [AutoData]
        public void Sum_TakesSumOfElements(Guid nodeOneId, Guid nodeTwoId, Guid nodeThreeId)
        {
            var additions = new List<CounterElement> { new(7, nodeOneId), new(17, nodeTwoId), new(9, nodeThreeId) };
            var subtractions = new List<CounterElement> { new(2, nodeOneId), new(3, nodeTwoId), new(1, nodeThreeId) };

            _repository.PersistAdditions(additions);
            _repository.PersistSubtractions(subtractions);

           _service.Merge(additions.ToImmutableHashSet(), subtractions.ToImmutableHashSet());

            var sum = _service.Sum();

            Assert.Equal(27, sum);
        }
    }
}