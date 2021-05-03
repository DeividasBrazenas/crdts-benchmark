using System;
using System.Collections.Generic;
using AutoFixture.Xunit2;
using CRDT.Application.Commutative;
using CRDT.Application.Commutative.Counter;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Counters.Entities;
using Xunit;

namespace CRDT.Application.UnitTests.Commutative
{
    public class G_CounterServiceTests
    {
        private readonly G_CounterRepository _repository;
        private readonly G_CounterService _service;

        public G_CounterServiceTests()
        {
            _repository = new G_CounterRepository();
            _service = new G_CounterService(_repository);
        }

        [Theory]
        [AutoData]
        public void Add_AddsElementToRepository(int value, Guid nodeId)
        {
            _service.Add(value, nodeId);

            Assert.Single(_repository.Elements);
            Assert.Contains(_repository.Elements, e => e.Value == value && e.Node.Id == nodeId);
        }

        [Theory]
        [AutoData]
        public void Add_UpdatesElementInRepository(List<CounterElement> elements, int value, Guid nodeId)
        {
            elements.Add(new CounterElement(999, nodeId));
            _repository.PersistValues(elements);

            _service.Add(value, nodeId);

            Assert.Equal(elements.Count, _repository.Elements.Count);
            Assert.Contains(_repository.Elements, e => e.Value == 999 + value && e.Node.Id == nodeId);
        }

        [Theory]
        [AutoData]
        public void Sum_TakesSumOfElements(Guid nodeOneId, Guid nodeTwoId, Guid nodeThreeId)
        {
            var elements = new List<CounterElement> { new(7, nodeOneId), new(17, nodeTwoId), new(9, nodeThreeId) };

            _repository.PersistValues(elements);

            var sum = _service.Sum();

            Assert.Equal(33, sum);
        }
    }
}