using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using Benchmarks.Framework;
using Benchmarks.Repositories;
using Benchmarks.TestTypes;
using CRDT.Core.Cluster;
using CRDT.Counters.Entities;
using Newtonsoft.Json.Linq;

namespace Benchmarks.Counters
{
    [SimpleJob(RunStrategy.Monitoring, RuntimeMoniker.NetCoreApp50, 1, 5, 10)]
    [MemoryDiagnoser]
    public class G_CounterBenchmarks
    {
        private List<Node> _nodes;
        private CRDT_Counter_Benchmarker<CRDT.Application.Convergent.Counter.G_CounterService> _convergentBenchmarker;
        private CRDT_Counter_Benchmarker<CRDT.Application.Commutative.Counter.G_CounterService> _commutativeBenchmarker;

        [Params(100)]
        public int Iterations;

        [IterationSetup]
        public void Setup()
        {
            _nodes = Node.CreateNodes(3);

            _convergentBenchmarker =
                new CRDT_Counter_Benchmarker<CRDT.Application.Convergent.Counter.G_CounterService>(
                    Iterations, _nodes, CreateConvergentReplicas(_nodes))
                {
                    Add = ConvergentAdd
                };

            _commutativeBenchmarker =
                new CRDT_Counter_Benchmarker<CRDT.Application.Commutative.Counter.G_CounterService>(
                    Iterations, _nodes, CreateCommutativeReplicas(_nodes))
                {
                    Add = CommutativeAdd
                };
        }

        [Benchmark]
        public void Convergent_Add()
        {
            _convergentBenchmarker.Benchmark_Add();
        }

        [Benchmark]
        public void Commutative_Add()
        {
            _commutativeBenchmarker.Benchmark_Add();
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

        private void CommutativeAdd(CRDT.Application.Commutative.Counter.G_CounterService sourceReplica, Guid replicaId, int value, List<CRDT.Application.Commutative.Counter.G_CounterService> downstreamReplicas)
        {
            sourceReplica.DownstreamAdd(value, replicaId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamAdd(value, replicaId);
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

        private void ConvergentAdd(CRDT.Application.Convergent.Counter.G_CounterService sourceReplica, Guid replicaId, int value, List<CRDT.Application.Convergent.Counter.G_CounterService> downstreamReplicas)
        {
            sourceReplica.LocalAdd(value, replicaId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Merge(sourceReplica.State);
            }
        }

        #endregion
    }
}