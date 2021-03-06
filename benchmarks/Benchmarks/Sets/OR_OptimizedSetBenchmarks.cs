using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using Benchmarks.Framework;
using Benchmarks.Repositories;
using Benchmarks.TestTypes;
using CRDT.Core.Cluster;

namespace Benchmarks.Sets
{
    [SimpleJob(RunStrategy.Monitoring, RuntimeMoniker.NetCoreApp50, 1, 5, 10)]
    [MemoryDiagnoser]
    public class OR_OptimizedSetBenchmarks
    {
        private List<Node> _nodes;
        private CRDT_Set_Benchmarker<CRDT.Application.Convergent.Set.OR_OptimizedSetService<TestType>> _convergentBenchmarker;
        private CRDT_Set_Benchmarker<CRDT.Application.Commutative.Set.OR_OptimizedSetService<TestType>> _commutativeBenchmarker;

        [Params(100)]
        public int Iterations;

        [IterationSetup]
        public void Setup()
        {
            _nodes = Node.CreateNodes(3);

            _convergentBenchmarker =
                new CRDT_Set_Benchmarker<CRDT.Application.Convergent.Set.OR_OptimizedSetService<TestType>>(
                    Iterations, _nodes, CreateConvergentReplicas(_nodes))
                {
                    Add = ConvergentAdd,
                    Remove = ConvergentRemove
                };

            _commutativeBenchmarker =
                new CRDT_Set_Benchmarker<CRDT.Application.Commutative.Set.OR_OptimizedSetService<TestType>>(
                    Iterations, _nodes, CreateCommutativeReplicas(_nodes))
                {
                    Add = CommutativeAdd,
                    Remove = CommutativeRemove,
                };
        }

        [Benchmark]
        public void Convergent_AddNewValue()
        {
            _convergentBenchmarker.Benchmark_Add();
        }

        [Benchmark]
        public void Commutative_AddNewValue()
        {
            _commutativeBenchmarker.Benchmark_Add();
        }

        [Benchmark]
        public void Convergent_AddAndRemoveValue()
        {
            _convergentBenchmarker.Benchmark_AddAndRemove();
        }

        [Benchmark]
        public void Commutative_AddAndRemoveValue()
        {
            _commutativeBenchmarker.Benchmark_AddAndRemove();
        }

        #region Commutative

        private Dictionary<Node, CRDT.Application.Commutative.Set.OR_OptimizedSetService<TestType>> CreateCommutativeReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Commutative.Set.OR_OptimizedSetService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new OR_OptimizedSetRepository();
                var service = new CRDT.Application.Commutative.Set.OR_OptimizedSetService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void CommutativeAdd(CRDT.Application.Commutative.Set.OR_OptimizedSetService<TestType> sourceReplica, TestType value,  List<CRDT.Application.Commutative.Set.OR_OptimizedSetService<TestType>> downstreamReplicas)
        {
            var tag = Guid.NewGuid();
            sourceReplica.LocalAdd(value, tag);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamAdd(value, tag);
            }
        }

        private void CommutativeRemove(CRDT.Application.Commutative.Set.OR_OptimizedSetService<TestType> sourceReplica, TestType value,  List<CRDT.Application.Commutative.Set.OR_OptimizedSetService<TestType>> downstreamReplicas)
        {
            var observedTags = sourceReplica.GetTags(value.Id);
            sourceReplica.LocalRemove(value, observedTags);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamRemove(value, observedTags);
            }
        }

        #endregion

        #region Convergent

        private Dictionary<Node, CRDT.Application.Convergent.Set.OR_OptimizedSetService<TestType>> CreateConvergentReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Convergent.Set.OR_OptimizedSetService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new OR_OptimizedSetRepository();
                var service = new CRDT.Application.Convergent.Set.OR_OptimizedSetService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void ConvergentAdd(CRDT.Application.Convergent.Set.OR_OptimizedSetService<TestType> sourceReplica, TestType value,  List<CRDT.Application.Convergent.Set.OR_OptimizedSetService<TestType>> downstreamReplicas)
        {
            sourceReplica.LocalAdd(value, Guid.NewGuid());

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Merge(sourceReplica.State);
            }
        }

        private void ConvergentRemove(CRDT.Application.Convergent.Set.OR_OptimizedSetService<TestType> sourceReplica, TestType value,  List<CRDT.Application.Convergent.Set.OR_OptimizedSetService<TestType>> downstreamReplicas)
        {
            var observedTags = sourceReplica.GetTags(value.Id);
            sourceReplica.LocalRemove(value, observedTags);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Merge(sourceReplica.State);
            }
        }

        #endregion
    }
}