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
    public class OUR_OptimizedSetBenchmarks
    {
        private List<Node> _nodes;
        private List<TestType> _objects;
        private CRDT_Set_Benchmarker<CRDT.Application.Convergent.Set.OUR_OptimizedSetService<TestType>> _convergentBenchmarker;
        private CRDT_Set_Benchmarker<CRDT.Application.Commutative.Set.OUR_OptimizedSetService<TestType>> _commutativeBenchmarker;

        [Params(100)]
        public int Iterations;

        [IterationSetup]
        public void Setup()
        {
            _nodes = Node.CreateNodes(3);
            _objects = new TestTypeBuilder(new Random()).Build(Guid.NewGuid(), _nodes.Count * Iterations * 2);

            _convergentBenchmarker =
                new CRDT_Set_Benchmarker<CRDT.Application.Convergent.Set.OUR_OptimizedSetService<TestType>>(
                    Iterations, _nodes, CreateConvergentReplicas(_nodes), _objects)
                {
                    AddWithTimestamp = ConvergentAdd,
                    UpdateWithTimestamp = ConvergentUpdate,
                    RemoveWithTimestamp = ConvergentRemove
                };

            _commutativeBenchmarker =
                new CRDT_Set_Benchmarker<CRDT.Application.Commutative.Set.OUR_OptimizedSetService<TestType>>(
                    Iterations, _nodes, CreateCommutativeReplicas(_nodes), _objects)
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

        private Dictionary<Node, CRDT.Application.Commutative.Set.OUR_OptimizedSetService<TestType>> CreateCommutativeReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Commutative.Set.OUR_OptimizedSetService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new OUR_OptimizedSetRepository();
                var service = new CRDT.Application.Commutative.Set.OUR_OptimizedSetService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void CommutativeAdd(CRDT.Application.Commutative.Set.OUR_OptimizedSetService<TestType> sourceReplica, TestType value, long timestamp, List<CRDT.Application.Commutative.Set.OUR_OptimizedSetService<TestType>> downstreamReplicas)
        {
            var tag = Guid.NewGuid();
            sourceReplica.LocalAdd(value, tag, timestamp);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamAdd(value, tag, timestamp);
            }
        }

        private void CommutativeUpdate(CRDT.Application.Commutative.Set.OUR_OptimizedSetService<TestType> sourceReplica, TestType value, long timestamp, List<CRDT.Application.Commutative.Set.OUR_OptimizedSetService<TestType>> downstreamReplicas)
        {
            var observedTags = sourceReplica.GetTags(value.Id);
            sourceReplica.LocalUpdate(value, observedTags, timestamp);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamUpdate(value, observedTags, timestamp);
            }
        }

        private void CommutativeRemove(CRDT.Application.Commutative.Set.OUR_OptimizedSetService<TestType> sourceReplica, TestType value, long timestamp, List<CRDT.Application.Commutative.Set.OUR_OptimizedSetService<TestType>> downstreamReplicas)
        {
            var observedTags = sourceReplica.GetTags(value.Id);
            sourceReplica.LocalRemove(value, observedTags, timestamp);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamRemove(value, observedTags, timestamp);
            }
        }
        
        #endregion

        #region Convergent

        private Dictionary<Node, CRDT.Application.Convergent.Set.OUR_OptimizedSetService<TestType>> CreateConvergentReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Convergent.Set.OUR_OptimizedSetService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new OUR_OptimizedSetRepository();
                var service = new CRDT.Application.Convergent.Set.OUR_OptimizedSetService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void ConvergentAdd(CRDT.Application.Convergent.Set.OUR_OptimizedSetService<TestType> sourceReplica, TestType value, long timestamp, List<CRDT.Application.Convergent.Set.OUR_OptimizedSetService<TestType>> downstreamReplicas)
        {
            sourceReplica.LocalAdd(value, Guid.NewGuid(), timestamp);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Merge(sourceReplica.State);
            }
        }

        private void ConvergentUpdate(CRDT.Application.Convergent.Set.OUR_OptimizedSetService<TestType> sourceReplica, TestType value, long timestamp, List<CRDT.Application.Convergent.Set.OUR_OptimizedSetService<TestType>> downstreamReplicas)
        {
            var observedTags = sourceReplica.GetTags(value.Id);
            sourceReplica.LocalUpdate(value, observedTags, timestamp);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Merge(sourceReplica.State);
            }
        }

        private void ConvergentRemove(CRDT.Application.Convergent.Set.OUR_OptimizedSetService<TestType> sourceReplica, TestType value, long timestamp, List<CRDT.Application.Convergent.Set.OUR_OptimizedSetService<TestType>> downstreamReplicas)
        {
            var observedTags = sourceReplica.GetTags(value.Id);
            sourceReplica.LocalRemove(value, observedTags, timestamp);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Merge(sourceReplica.State);
            }
        }
        
        #endregion
    }
}