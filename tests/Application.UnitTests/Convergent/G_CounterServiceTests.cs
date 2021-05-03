using System;
using System.Collections.Generic;
using AutoFixture.Xunit2;
using CRDT.Application.Convergent;
using CRDT.Application.Convergent.Counter;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Counters.Entities;
using Xunit;

namespace CRDT.Application.UnitTests.Convergent
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
        public void Merge_TakesMaxValues(Guid nodeOneId, Guid nodeTwoId, Guid nodeThreeId, Guid nodeFourId)
        {
            var existingElements = new List<CounterElement> { new(7, nodeOneId), new(17, nodeTwoId), new(9, nodeThreeId) };
            var elements = new List<CounterElement> { new(3, nodeTwoId), new(42, nodeThreeId), new(10, nodeFourId) };

            _repository.PersistValues(existingElements);

            _service.Merge(elements);

            Assert.Equal(4, _repository.Elements.Count);
            Assert.Contains(_repository.Elements, e => e.Value == 7 && e.Node.Id == nodeOneId);
            Assert.Contains(_repository.Elements, e => e.Value == 17 && e.Node.Id == nodeTwoId);
            Assert.Contains(_repository.Elements, e => e.Value == 42 && e.Node.Id == nodeThreeId);
            Assert.Contains(_repository.Elements, e => e.Value == 10 && e.Node.Id == nodeFourId);
        }

        [Theory]
        [AutoData]
        public void Merge_IsCommutative(Guid nodeOneId, Guid nodeTwoId, Guid nodeThreeId, Guid nodeFourId)
        {
            var existingElements = new List<CounterElement> { new(7, nodeOneId), new(17, nodeTwoId), new(9, nodeThreeId) };
            var elements = new List<CounterElement> { new(3, nodeTwoId), new(42, nodeThreeId), new(10, nodeFourId) };

            _repository.PersistValues(existingElements);

            _service.Merge(elements);
            _service.Merge(elements);
            _service.Merge(elements);
            _service.Merge(elements);

            Assert.Equal(4, _repository.Elements.Count);
            Assert.Contains(_repository.Elements, e => e.Value == 7 && e.Node.Id == nodeOneId);
            Assert.Contains(_repository.Elements, e => e.Value == 17 && e.Node.Id == nodeTwoId);
            Assert.Contains(_repository.Elements, e => e.Value == 42 && e.Node.Id == nodeThreeId);
            Assert.Contains(_repository.Elements, e => e.Value == 10 && e.Node.Id == nodeFourId);
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