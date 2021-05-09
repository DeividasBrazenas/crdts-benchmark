using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using Benchmarks.Framework;
using Benchmarks.Repositories;
using Benchmarks.TestTypes;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;

namespace Benchmarks.Sets
{
    [SimpleJob(RunStrategy.Monitoring, RuntimeMoniker.NetCoreApp50, 1, 5, 10)]
    [MemoryDiagnoser]
    public class OUR_SetWithVCBenchmarks
    {
        private List<Node> _nodes;
        private CRDT_Set_Benchmarker<CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType>> _convergentBenchmarker;
        private CRDT_Set_Benchmarker<CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType>> _commutativeBenchmarker;

        [Params(100)]
        public int Iterations;

        [IterationSetup]
        public void Setup()
        {
            _nodes = Node.CreateNodes(3);

            _convergentBenchmarker =
                new CRDT_Set_Benchmarker<CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType>>(
                    Iterations, _nodes, CreateConvergentReplicas(_nodes))
                {
                    AddWithVectorClock = ConvergentAdd,
                    UpdateWithVectorClock = ConvergentUpdate,
                    RemoveWithVectorClock = ConvergentRemove
                };

            _commutativeBenchmarker =
                new CRDT_Set_Benchmarker<CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType>>(
                    Iterations, _nodes, CreateCommutativeReplicas(_nodes))
                {
                    AddWithVectorClock = CommutativeAdd,
                    UpdateWithVectorClock = CommutativeUpdate,
                    RemoveWithVectorClock = CommutativeRemove,
                };
        }

        [Benchmark]
        public void Convergent_AddNewValue()
        {
            _convergentBenchmarker.Benchmark_Add_WithVectorClock();
        }

        [Benchmark]
        public void Commutative_AddNewValue()
        {
            _commutativeBenchmarker.Benchmark_Add_WithVectorClock();
        }

        [Benchmark]
        public void Convergent_AddAndUpdateValue()
        {
            _convergentBenchmarker.Benchmark_AddAndUpdate_WithVectorClock();
        }

        [Benchmark]
        public void Commutative_AddAndUpdateValue()
        {
            _commutativeBenchmarker.Benchmark_AddAndUpdate_WithVectorClock();
        }

        [Benchmark]
        public void Convergent_AddAndRemoveValue()
        {
            _convergentBenchmarker.Benchmark_AddAndRemove_WithVectorClock();
        }

        [Benchmark]
        public void Commutative_AddAndRemoveValue()
        {
            _commutativeBenchmarker.Benchmark_AddAndRemove_WithVectorClock();
        }

        [Benchmark]
        public void Convergent_AddUpdateAndRemoveValue()
        {
            _convergentBenchmarker.Benchmark_AddUpdateAndRemove_WithVectorClock();
        }

        [Benchmark]
        public void Commutative_AddUpdateAndRemoveValue()
        {
            _commutativeBenchmarker.Benchmark_AddUpdateAndRemove_WithVectorClock();
        }

        #region Commutative

        private Dictionary<Node, CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType>> CreateCommutativeReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new OUR_SetWithVCRepository();
                var service = new CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void CommutativeAdd(CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType> sourceReplica, TestType value, VectorClock vectorClock, List<CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType>> downstreamReplicas)
        {
            var tag = Guid.NewGuid();
            sourceReplica.LocalAdd(value, tag, vectorClock);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamAdd(value, tag, vectorClock);
            }
        }

        private void CommutativeUpdate(CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType> sourceReplica, TestType value, VectorClock vectorClock, List<CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType>> downstreamReplicas)
        {
            var observedTags = sourceReplica.GetTags(value.Id);
            sourceReplica.LocalUpdate(value, observedTags, vectorClock);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamUpdate(value, observedTags, vectorClock);
            }
        }

        private void CommutativeRemove(CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType> sourceReplica, TestType value, VectorClock vectorClock, List<CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType>> downstreamReplicas)
        {
            var observedTags = sourceReplica.GetTags(value.Id);
            sourceReplica.LocalRemove(value, observedTags, vectorClock);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamRemove(value, observedTags, vectorClock);
            }
        }
        #endregion

        #region Convergent

        private Dictionary<Node, CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType>> CreateConvergentReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new OUR_SetWithVCRepository();
                var service = new CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void ConvergentAdd(CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType> sourceReplica, TestType value, VectorClock vectorClock, List<CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType>> downstreamReplicas)
        {
            sourceReplica.LocalAdd(value, Guid.NewGuid(), vectorClock);

            var (adds, removes) = sourceReplica.State;

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Merge(adds, removes);
            }
        }

        private void ConvergentUpdate(CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType> sourceReplica, TestType value, VectorClock vectorClock, List<CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType>> downstreamReplicas)
        {
            var observedTags = sourceReplica.GetTags(value.Id);
            sourceReplica.LocalUpdate(value, observedTags, vectorClock);

            var (adds, removes) = sourceReplica.State;

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Merge(adds, removes);
            }
        }

        private void ConvergentRemove(CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType> sourceReplica, TestType value, VectorClock vectorClock, List<CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType>> downstreamReplicas)
        {
            var observedTags = sourceReplica.GetTags(value.Id);
            sourceReplica.LocalRemove(value, observedTags, vectorClock);

            var (adds, removes) = sourceReplica.State;

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Merge(adds, removes);
            }
        }

        #endregion
    }
}