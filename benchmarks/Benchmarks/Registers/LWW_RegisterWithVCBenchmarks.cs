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
using CRDT.Core.DistributedTime;
using CRDT.Registers.Entities;
using Newtonsoft.Json.Linq;

namespace Benchmarks.Registers
{
    [SimpleJob(RunStrategy.Monitoring, RuntimeMoniker.NetCoreApp50, 1, 5, 10)]
    [MemoryDiagnoser]
    public class LWW_RegisterWithVCBenchmarks
    {
        private List<Node> _nodes;
        private CRDT_Register_Benchmarker<CRDT.Application.Convergent.Register.LWW_RegisterWithVCService<TestType>> _convergentBenchmarker;
        private CRDT_Register_Benchmarker<CRDT.Application.Commutative.Register.LWW_RegisterWithVCService<TestType>> _commutativeBenchmarker;

        [Params(100)]
        public int Iterations;

        [IterationSetup]
        public void Setup()
        {
            _nodes = Node.CreateNodes(3);

            _convergentBenchmarker =
                new CRDT_Register_Benchmarker<CRDT.Application.Convergent.Register.LWW_RegisterWithVCService<TestType>>(
                    Iterations, _nodes, CreateConvergentReplicas(_nodes))
                {
                    ConvergentAssignWithVectorClock = ConvergentAssign,
                    ConvergentRemoveWithVectorClock = ConvergentRemove
                };

            _commutativeBenchmarker =
                new CRDT_Register_Benchmarker<CRDT.Application.Commutative.Register.LWW_RegisterWithVCService<TestType>>(
                    Iterations, _nodes, CreateCommutativeReplicas(_nodes))
                {
                    CommutativeAssignWithVectorClock = CommutativeAssign,
                    CommutativeRemoveWithVectorClock = CommutativeRemove
                };
        }

        [Benchmark]
        public void Convergent_AssignValue()
        {
            _convergentBenchmarker.Benchmark_Convergent_Assign_WithVectorClock();
        }

        [Benchmark]
        public void Commutative_AssignValue()
        {
            _commutativeBenchmarker.Benchmark_Commutative_Assign_WithVectorClock();
        }

        [Benchmark]
        public void Convergent_AssignSingleProperty()
        {
            _convergentBenchmarker.Benchmark_Convergent_AssignSingleProperty_WithVectorClock();
        }

        [Benchmark]
        public void Commutative_AssignSingleProperty()
        {
            _commutativeBenchmarker.Benchmark_Commutative_AssignSingleProperty_WithVectorClock();
        }

        [Benchmark]
        public void Convergent_AssignAndRemoveValue()
        {
            _convergentBenchmarker.Benchmark_Convergent_AssignAndRemove_WithVectorClock();
        }

        [Benchmark]
        public void Commutative_AssignAndRemoveValue()
        {
            _commutativeBenchmarker.Benchmark_Commutative_AssignAndRemove_WithVectorClock();
        }

        #region Commutative

        private Dictionary<Node, CRDT.Application.Commutative.Register.LWW_RegisterWithVCService<TestType>> CreateCommutativeReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Commutative.Register.LWW_RegisterWithVCService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new LWW_RegisterWithVCRepository();
                var service = new CRDT.Application.Commutative.Register.LWW_RegisterWithVCService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void CommutativeAssign(CRDT.Application.Commutative.Register.LWW_RegisterWithVCService<TestType> sourceReplica, Guid id, JToken value, VectorClock clock, List<CRDT.Application.Commutative.Register.LWW_RegisterWithVCService<TestType>> downstreamReplicas)
        {
            sourceReplica.LocalAssign(id, value, clock);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamAssign(id, value, clock);
            }
        }

        private void CommutativeRemove(CRDT.Application.Commutative.Register.LWW_RegisterWithVCService<TestType> sourceReplica, TestType value, VectorClock clock, List<CRDT.Application.Commutative.Register.LWW_RegisterWithVCService<TestType>> downstreamReplicas)
        {
            sourceReplica.LocalRemove(value, clock);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamRemove(value, clock);
            }
        }

        #endregion

        #region Convergent

        private Dictionary<Node, CRDT.Application.Convergent.Register.LWW_RegisterWithVCService<TestType>> CreateConvergentReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Convergent.Register.LWW_RegisterWithVCService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new LWW_RegisterWithVCRepository();
                var service = new CRDT.Application.Convergent.Register.LWW_RegisterWithVCService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void ConvergentAssign(CRDT.Application.Convergent.Register.LWW_RegisterWithVCService<TestType> sourceReplica, TestType value, VectorClock clock, List<CRDT.Application.Convergent.Register.LWW_RegisterWithVCService<TestType>> downstreamReplicas)
        {
            sourceReplica.LocalAssign(value, clock);

            var state = sourceReplica.GetValue(value.Id);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamAssign(state.Value, state.VectorClock);
            }
        }

        private void ConvergentRemove(CRDT.Application.Convergent.Register.LWW_RegisterWithVCService<TestType> sourceReplica, TestType value, VectorClock clock, List<CRDT.Application.Convergent.Register.LWW_RegisterWithVCService<TestType>> downstreamReplicas)
        {
            sourceReplica.LocalRemove(value, clock);

            var state = sourceReplica.GetValue(value.Id);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamRemove(state.Value, state.VectorClock);
            }
        }

        #endregion
    }
}