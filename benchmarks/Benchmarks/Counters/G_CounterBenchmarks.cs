using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using CRDT.Benchmarks.Repositories;
using CRDT.Core.Cluster;
using CRDT.Counters.Entities;

namespace Benchmarks.Counters
{
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    public class G_CounterBenchmarks
    {
        private List<Node> _nodes;
        private Dictionary<Node, CRDT.Application.Commutative.Counter.G_CounterService> _commutativeReplicas;
        private Dictionary<Node, CRDT.Application.Convergent.Counter.G_CounterService> _convergentReplicas;

        [GlobalSetup]
        public void Setup()
        {
            _nodes = CreateNodes(3);
            _commutativeReplicas = CreateCommutativeReplicas(_nodes);
            _convergentReplicas = CreateConvergentReplicas(_nodes);
        }

        [Benchmark]
        public void Commutative_Add_NetworkOK()
        {
            for (int i = 1; i <= 100; i++)
            {
                Parallel.ForEach(_commutativeReplicas, replica =>
                {
                    replica.Value.Add(i, replica.Key.Id);

                    CommutativeDownstreamAdd(replica.Key.Id, i);
                });
            }
        }

        [Benchmark]
        public void Convergent_Add_NetworkOK()
        {
            for (int i = 1; i <= 100; i++)
            {
                Parallel.ForEach(_convergentReplicas, replica =>
                {
                    replica.Value.LocalAdd(i, replica.Key.Id);

                    ConvergentDownstreamMerge(replica.Key.Id, replica.Value.State);
                });
            }
        }

        [Benchmark]
        public void Convergent_Add_Every2ndNodeDidNotReceiveUpdateImmediately()
        {
            for (int i = 1; i <= 100; i++)
            {
                Parallel.ForEach(_convergentReplicas, replica =>
                {
                    replica.Value.LocalAdd(i, replica.Key.Id);

                    ConvergentMergeDownstreamWithNetworkFailures(replica.Key.Id, replica.Value.State);
                });
            }
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

        #region Commutative

        private Dictionary<Node, CRDT.Application.Commutative.Counter.G_CounterService> CreateCommutativeReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Commutative.Counter.G_CounterService>();

            foreach (var node in nodes)
            {
                var repository = new G_CounterRepository();
                var service = new CRDT.Application.Commutative.Counter.G_CounterService(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void CommutativeDownstreamAdd(Guid senderId, int value)
        {
            var downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.Add(value, senderId);
            }
        }
        #endregion

        #region Convergent

        private Dictionary<Node, CRDT.Application.Convergent.Counter.G_CounterService> CreateConvergentReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Convergent.Counter.G_CounterService>();

            foreach (var node in nodes)
            {
                var repository = new G_CounterRepository();
                var service = new CRDT.Application.Convergent.Counter.G_CounterService(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void ConvergentDownstreamMerge(Guid senderId, IEnumerable<CounterElement> state)
        {
            var downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.Merge(state);
            }
        }

        private void ConvergentMergeDownstreamWithNetworkFailures(Guid senderId, IEnumerable<CounterElement> state)
        {
            var downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != senderId).Where((x, i) => i % 2 == 0);
            var replicasWithoutUpdate = _convergentReplicas.Except(downstreamReplicas).Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.Merge(state);

                foreach (var replicaWithoutUpdate in replicasWithoutUpdate)
                {
                    replicaWithoutUpdate.Value.Merge(downstreamReplica.Value.State);
                }
            }
        }

        #endregion
    }
}