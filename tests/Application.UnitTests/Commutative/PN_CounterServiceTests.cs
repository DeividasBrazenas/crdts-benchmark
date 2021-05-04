using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Commutative.Counter;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Counters.Entities;
using Xunit;

namespace CRDT.Application.UnitTests.Commutative
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
        public void Add_AddsElementToRepository(int value, Guid nodeId)
        {
            _service.Add(value, nodeId);

            Assert.Single(_repository.Additions);
            Assert.Contains(_repository.Additions, e => e.Value == value && e.Node.Id == nodeId);
        }

        [Theory]
        [AutoData]
        public void Add_UpdatesElementInRepository(List<CounterElement> elements, int value, Guid nodeId)
        {
            elements.Add(new CounterElement(999, nodeId));
            _repository.PersistAdditions(elements);

            _service.Add(value, nodeId);

            Assert.Equal(elements.Count, _repository.Additions.ToList().Count);
            Assert.Contains(_repository.Additions, e => e.Value == 999 + value && e.Node.Id == nodeId);
        }

        [Theory]
        [AutoData]
        public void Subtract_AddsElementToRepository(int value, Guid nodeId)
        {
            _service.Subtract(value, nodeId);

            Assert.Single(_repository.Subtractions);
            Assert.Contains(_repository.Subtractions, e => e.Value == value && e.Node.Id == nodeId);
        }

        [Theory]
        [AutoData]
        public void Subtract_UpdatesElementInRepository(List<CounterElement> elements, int value, Guid nodeId)
        {
            elements.Add(new CounterElement(999, nodeId));
            _repository.PersistSubtractions(elements);

            _service.Subtract(value, nodeId);

            Assert.Equal(elements.Count, _repository.Subtractions.ToList().Count);
            Assert.Contains(_repository.Subtractions, e => e.Value == 999 + value && e.Node.Id == nodeId);
        }

        [Theory]
        [AutoData]
        public void Sum_TakesSumOfElements(Guid nodeOneId, Guid nodeTwoId, Guid nodeThreeId)
        {
            var elements = new List<CounterElement> { new(7, nodeOneId), new(17, nodeTwoId), new(9, nodeThreeId) };
            var subtractions = new List<CounterElement> { new(2, nodeOneId), new(3, nodeTwoId), new(1, nodeThreeId) };

            _repository.PersistAdditions(elements);
            _repository.PersistSubtractions(subtractions);

            var sum = _service.Sum();

            Assert.Equal(27, sum);
        }
    }
}