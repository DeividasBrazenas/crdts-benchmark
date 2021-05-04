using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using CRDT.Application.Commutative.Counter;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Core.Cluster;
using CRDT.Counters.Entities;
using Xunit;
using Xunit.Abstractions;

namespace CRDT.Application.UnitTests.Commutative
{
    public class G_CounterServiceTests
    {
        private readonly G_CounterRepository _repository;
        private readonly G_CounterService _service;
        private readonly ITestOutputHelper _output;

        public G_CounterServiceTests(ITestOutputHelper output)
        {
            _repository = new G_CounterRepository();
            _service = new G_CounterService(_repository);
            _output = output;
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

            Assert.Equal(elements.Count, _repository.Elements.ToList().Count);
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

        [Fact]
        public void Increment_BenchmarkNumberOne()
        {
            var nodes = CreateNodes(3);
            var replicas = CreateReplicas(nodes);

            for (int i = 1; i <= 100; i++)
            {
                Parallel.ForEach(replicas, replica =>
                {
                    SendAdd(replica.Key.Id, i, replicas);
                });
            }

            VerifyReplicas(replicas, 15150);
        }

        private List<Node> CreateNodes(int count)
        {
            var nodes = new List<Node>();

            for (var i = 0; i < count; i++)
            {
                nodes.Add(new Node());
            }

            return nodes;
        }

        private Dictionary<Node, G_CounterService> CreateReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, G_CounterService>();

            foreach (var node in nodes)
            {
                var repository = new G_CounterRepository();
                var service = new G_CounterService(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void SendAdd(Guid senderId, int value, Dictionary<Node, G_CounterService> replicas)
        {
            foreach (var replica in replicas)
            {
                replica.Value.Add(value, senderId);
                // _output.WriteLine($"Adding {senderId}:{value} to {replica.Key.Id}. Replica values: {JsonConvert.SerializeObject(replica.Value._repository.GetValues())}");

            }
        }

        private void VerifyReplicas(Dictionary<Node, G_CounterService> replicas, int expectedSum)
        {
            foreach (var replica in replicas)
            {
                var sum = replica.Value.Sum();

                //_output.WriteLine($"Replica's sum is {sum}");
                if (expectedSum != sum)
                {
                    throw new Exception($"Expected {expectedSum}, but got {sum}");
                }
            }
        }
    }
}