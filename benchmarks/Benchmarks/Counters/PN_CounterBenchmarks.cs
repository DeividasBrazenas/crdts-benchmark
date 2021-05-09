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
    public class PN_CounterBenchmarks
    {
        private List<Node> _nodes;
        private CRDT_Counter_Benchmarker<CRDT.Application.Convergent.Counter.PN_CounterService> _convergentBenchmarker;
        private CRDT_Counter_Benchmarker<CRDT.Application.Commutative.Counter.PN_CounterService> _commutativeBenchmarker;

        [Params(100)]
        public int Iterations;

        [IterationSetup]
        public void Setup()
        {
            _nodes = Node.CreateNodes(3);

            _convergentBenchmarker =
                new CRDT_Counter_Benchmarker<CRDT.Application.Convergent.Counter.PN_CounterService>(
                    Iterations, _nodes, CreateConvergentReplicas(_nodes))
                {
                    Add = ConvergentAdd,
                    Subtract = ConvergentSubtract
                };

            _commutativeBenchmarker =
                new CRDT_Counter_Benchmarker<CRDT.Application.Commutative.Counter.PN_CounterService>(
                    Iterations, _nodes, CreateCommutativeReplicas(_nodes))
                {
                    Add = CommutativeAdd,
                    Subtract = CommutativeSubtract
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

        [Benchmark]
        public void Convergent_Subtract()
        {
            _convergentBenchmarker.Benchmark_Subtract();
        }

        [Benchmark]
        public void Commutative_Subtract()
        {
            _commutativeBenchmarker.Benchmark_Subtract();
        }

        [Benchmark]
        public void Convergent_AddAndSubtract()
        {
            _convergentBenchmarker.Benchmark_AddAndSubtract();
        }

        [Benchmark]
        public void Commutative_AddAndSubtract()
        {
            _commutativeBenchmarker.Benchmark_AddAndSubtract();
        }

        #region Commutative

        private Dictionary<Node, CRDT.Application.Commutative.Counter.PN_CounterService> CreateCommutativeReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Commutative.Counter.PN_CounterService>();

            foreach (var node in nodes)
            {
                var repository = new PN_CounterRepository();
                var service = new CRDT.Application.Commutative.Counter.PN_CounterService(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void CommutativeAdd(CRDT.Application.Commutative.Counter.PN_CounterService sourceReplica, Guid replicaId, int value, List<CRDT.Application.Commutative.Counter.PN_CounterService> downstreamReplicas)
        {
            sourceReplica.DownstreamAdd(value, replicaId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamAdd(value, replicaId);
            }
        }

        private void CommutativeSubtract(CRDT.Application.Commutative.Counter.PN_CounterService sourceReplica, Guid replicaId, int value, List<CRDT.Application.Commutative.Counter.PN_CounterService> downstreamReplicas)
        {
            sourceReplica.DownstreamSubtract(value, replicaId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamSubtract(value, replicaId);
            }
        }

        #endregion

        #region Convergent

        private Dictionary<Node, CRDT.Application.Convergent.Counter.PN_CounterService> CreateConvergentReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Convergent.Counter.PN_CounterService>();

            foreach (var node in nodes)
            {
                var repository = new PN_CounterRepository();
                var service = new CRDT.Application.Convergent.Counter.PN_CounterService(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void ConvergentAdd(CRDT.Application.Convergent.Counter.PN_CounterService sourceReplica, Guid replicaId, int value, List<CRDT.Application.Convergent.Counter.PN_CounterService> downstreamReplicas)
        {
            sourceReplica.LocalAdd(value, replicaId);

            var (adds, subtracts) = sourceReplica.State;

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Merge(adds, subtracts);
            }
        }

        private void ConvergentSubtract(CRDT.Application.Convergent.Counter.PN_CounterService sourceReplica, Guid replicaId, int value, List<CRDT.Application.Convergent.Counter.PN_CounterService> downstreamReplicas)
        {
            sourceReplica.LocalSubtract(value, replicaId);

            var (adds, subtracts) = sourceReplica.State;

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Merge(adds, subtracts);
            }
        }

        #endregion
    }
}