using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using CRDT.Application.Convergent.Counter;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Core.Cluster;
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

            Assert.Equal(4, _repository.Elements.ToList().Count);
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

            Assert.Equal(4, _repository.Elements.ToList().Count);
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

        [Fact]
        public void Add_NetworkOK()
        {
            var nodes = CreateNodes(3);
            var replicas = CreateReplicas(nodes);

            for (int i = 1; i <= 100; i++)
            {
                Parallel.ForEach(replicas, replica =>
                {
                    replica.Value.LocalAdd(i, replica.Key.Id);

                    DownstreamMerge(replica.Key.Id, replica.Value.State, replicas);
                });
            }

            VerifyReplicas(replicas, 15150);
        }

        [Fact]
        public void Add_WithNetworkFailures()
        {
            var nodes = CreateNodes(3);
            var replicas = CreateReplicas(nodes);

            for (int i = 1; i <= 100; i++)
            {
                Parallel.ForEach(replicas, replica =>
                {
                    replica.Value.LocalAdd(i, replica.Key.Id);

                    DownstreamMergeWithNetworkFailures(replica.Key.Id, replica.Value.State, replicas);
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


        private void DownstreamMerge(Guid senderId, IEnumerable<CounterElement> state, Dictionary<Node, G_CounterService> replicas)
        {
            var downstreamReplicas = replicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.Merge(state);
            }
        }

        private void DownstreamMergeWithNetworkFailures(Guid senderId, IEnumerable<CounterElement> state, Dictionary<Node, G_CounterService> replicas)
        {
            var downstreamReplicas = replicas.Where(r => r.Key.Id != senderId).Where((x, i) => i % 2 == 0);
            var replicasWithoutUpdate = replicas.Except(downstreamReplicas).Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.Merge(state);

                foreach (var replicaWithoutUpdate in replicasWithoutUpdate)
                {
                    replicaWithoutUpdate.Value.Merge(downstreamReplica.Value.State);
                }
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