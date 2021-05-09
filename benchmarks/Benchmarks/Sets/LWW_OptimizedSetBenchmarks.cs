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
using CRDT.Sets.Entities;
using Newtonsoft.Json.Linq;

namespace Benchmarks.Sets
{
    [SimpleJob(RunStrategy.Monitoring, RuntimeMoniker.NetCoreApp50, 1, 5, 10)]
    [MemoryDiagnoser]
    public class LWW_OptimizedSetBenchmarks
    {
        private List<Node> _nodes;
        private CRDT_Set_Benchmarker<CRDT.Application.Convergent.Set.LWW_OptimizedSetService<TestType>> _convergentBenchmarker;
        private CRDT_Set_Benchmarker<CRDT.Application.Commutative.Set.LWW_OptimizedSetService<TestType>> _commutativeBenchmarker;

        [Params(100)]
        public int Iterations;

        [IterationSetup]
        public void Setup()
        {
            _nodes = Node.CreateNodes(3);

            _convergentBenchmarker =
                new CRDT_Set_Benchmarker<CRDT.Application.Convergent.Set.LWW_OptimizedSetService<TestType>>(
                    Iterations, _nodes, CreateConvergentReplicas(_nodes))
                {
                    AddWithTimestamp = ConvergentAdd,
                    UpdateWithTimestamp = ConvergentUpdate,
                    RemoveWithTimestamp = ConvergentRemove
                };

            _commutativeBenchmarker =
                new CRDT_Set_Benchmarker<CRDT.Application.Commutative.Set.LWW_OptimizedSetService<TestType>>(
                    Iterations, _nodes, CreateCommutativeReplicas(_nodes))
                {
                    AddWithTimestamp = CommutativeAdd,
                    UpdateWithTimestamp = CommutativeUpdate,
                    RemoveWithTimestamp = CommutativeRemove,
                };
        }

        [Benchmark]
        public void Convergent_AddNewValue()
        {
            _convergentBenchmarker.Benchmark_Add_WithTimestamp();
        }

        [Benchmark]
        public void Commutative_AddNewValue()
        {
            _commutativeBenchmarker.Benchmark_Add_WithTimestamp();
        }

        [Benchmark]
        public void Convergent_AddAndUpdateValue()
        {
            _convergentBenchmarker.Benchmark_AddAndUpdate_WithTimestamp();
        }

        [Benchmark]
        public void Commutative_AddAndUpdateValue()
        {
            _commutativeBenchmarker.Benchmark_AddAndUpdate_WithTimestamp();
        }

        [Benchmark]
        public void Convergent_AddAndRemoveValue()
        {
            _convergentBenchmarker.Benchmark_AddAndRemove_WithTimestamp();
        }

        [Benchmark]
        public void Commutative_AddAndRemoveValue()
        {
            _commutativeBenchmarker.Benchmark_AddAndRemove_WithTimestamp();
        }

        [Benchmark]
        public void Convergent_AddUpdateAndRemoveValue()
        {
            _convergentBenchmarker.Benchmark_AddUpdateAndRemove_WithTimestamp();
        }

        [Benchmark]
        public void Commutative_AddUpdateAndRemoveValue()
        {
            _commutativeBenchmarker.Benchmark_AddUpdateAndRemove_WithTimestamp();
        }

        #region Commutative

        private Dictionary<Node, CRDT.Application.Commutative.Set.LWW_OptimizedSetService<TestType>> CreateCommutativeReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Commutative.Set.LWW_OptimizedSetService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new LWW_OptimizedSetRepository();
                var service = new CRDT.Application.Commutative.Set.LWW_OptimizedSetService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void CommutativeAdd(CRDT.Application.Commutative.Set.LWW_OptimizedSetService<TestType> sourceReplica, TestType value, long timestamp, List<CRDT.Application.Commutative.Set.LWW_OptimizedSetService<TestType>> downstreamReplicas)
        {
            sourceReplica.LocalAssign(value, timestamp);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamAssign(value, timestamp);
            }
        }

        private void CommutativeUpdate(CRDT.Application.Commutative.Set.LWW_OptimizedSetService<TestType> sourceReplica, TestType value, long timestamp, List<CRDT.Application.Commutative.Set.LWW_OptimizedSetService<TestType>> downstreamReplicas)
        {
            sourceReplica.LocalAssign(value, timestamp);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamAssign(value, timestamp);
            }
        }

        private void CommutativeRemove(CRDT.Application.Commutative.Set.LWW_OptimizedSetService<TestType> sourceReplica, TestType value, long timestamp, List<CRDT.Application.Commutative.Set.LWW_OptimizedSetService<TestType>> downstreamReplicas)
        {
            sourceReplica.LocalRemove(value, timestamp);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamRemove(value, timestamp);
            }
        }

        #endregion

        #region Convergent

        private Dictionary<Node, CRDT.Application.Convergent.Set.LWW_OptimizedSetService<TestType>> CreateConvergentReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Convergent.Set.LWW_OptimizedSetService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new LWW_OptimizedSetRepository();
                var service = new CRDT.Application.Convergent.Set.LWW_OptimizedSetService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void ConvergentAdd(CRDT.Application.Convergent.Set.LWW_OptimizedSetService<TestType> sourceReplica, TestType value, long timestamp, List<CRDT.Application.Convergent.Set.LWW_OptimizedSetService<TestType>> downstreamReplicas)
        {
            sourceReplica.LocalAssign(value, timestamp);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Merge(sourceReplica.State);
            }
        }

        private void ConvergentUpdate(CRDT.Application.Convergent.Set.LWW_OptimizedSetService<TestType> sourceReplica, TestType value, long timestamp, List<CRDT.Application.Convergent.Set.LWW_OptimizedSetService<TestType>> downstreamReplicas)
        {
            sourceReplica.LocalAssign(value, timestamp);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Merge(sourceReplica.State);
            }
        }

        private void ConvergentRemove(CRDT.Application.Convergent.Set.LWW_OptimizedSetService<TestType> sourceReplica, TestType value, long timestamp, List<CRDT.Application.Convergent.Set.LWW_OptimizedSetService<TestType>> downstreamReplicas)
        {
            sourceReplica.LocalRemove(value, timestamp);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Merge(sourceReplica.State);
            }
        }

        #endregion
    }
}