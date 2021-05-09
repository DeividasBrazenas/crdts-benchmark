using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using Benchmarks.Framework;
using Benchmarks.Repositories;
using Benchmarks.TestTypes;
using CRDT.Core.Cluster;
using Newtonsoft.Json.Linq;

namespace Benchmarks.Registers
{
    [SimpleJob(RunStrategy.Monitoring, RuntimeMoniker.NetCoreApp50, 1, 5, 10)]
    [MemoryDiagnoser]
    public class LWW_RegisterBenchmarks
    {
        private List<Node> _nodes;
        private CRDT_Register_Benchmarker<CRDT.Application.Convergent.Register.LWW_RegisterService<TestType>> _convergentBenchmarker;
        private CRDT_Register_Benchmarker<CRDT.Application.Commutative.Register.LWW_RegisterService<TestType>> _commutativeBenchmarker;

        [Params(100)]
        public int Iterations;

        [IterationSetup]
        public void Setup()
        {
            _nodes = Node.CreateNodes(3);

            _convergentBenchmarker =
                new CRDT_Register_Benchmarker<CRDT.Application.Convergent.Register.LWW_RegisterService<TestType>>(
                    Iterations, _nodes, CreateConvergentReplicas(_nodes))
                {
                    ConvergentAssignWithTimestamp = ConvergentAssign,
                    ConvergentRemoveWithTimestamp = ConvergentRemove
                };

            _commutativeBenchmarker =
                new CRDT_Register_Benchmarker<CRDT.Application.Commutative.Register.LWW_RegisterService<TestType>>(
                    Iterations, _nodes, CreateCommutativeReplicas(_nodes))
                {
                    CommutativeAssignWithTimestamp = CommutativeAssign,
                    CommutativeRemoveWithTimestamp = CommutativeRemove
                };
        }

        [Benchmark]
        public void Convergent_AssignValue()
        {
            _convergentBenchmarker.Benchmark_Convergent_Assign_WithTimestamp();
        }

        [Benchmark]
        public void Commutative_AssignValue()
        {
            _commutativeBenchmarker.Benchmark_Commutative_Assign_WithTimestamp();
        }

        [Benchmark]
        public void Convergent_AssignSingleProperty()
        {
            _convergentBenchmarker.Benchmark_Convergent_AssignSingleProperty_WithTimestamp();
        }

        [Benchmark]
        public void Commutative_AssignSingleProperty()
        {
            _commutativeBenchmarker.Benchmark_Commutative_AssignSingleProperty_WithTimestamp();
        }

        [Benchmark]
        public void Convergent_AssignAndRemoveValue()
        {
            _convergentBenchmarker.Benchmark_Convergent_AssignAndRemove_WithTimestamp();
        }

        [Benchmark]
        public void Commutative_AssignAndRemoveValue()
        {
            _commutativeBenchmarker.Benchmark_Commutative_AssignAndRemove_WithTimestamp();
        }

        #region Commutative

        private Dictionary<Node, CRDT.Application.Commutative.Register.LWW_RegisterService<TestType>> CreateCommutativeReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Commutative.Register.LWW_RegisterService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new LWW_RegisterRepository();
                var service = new CRDT.Application.Commutative.Register.LWW_RegisterService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void CommutativeAssign(CRDT.Application.Commutative.Register.LWW_RegisterService<TestType> sourceReplica, Guid id, JToken value, long timestamp, List<CRDT.Application.Commutative.Register.LWW_RegisterService<TestType>> downstreamReplicas)
        {
            sourceReplica.LocalAssign(id, value, timestamp);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamAssign(id, value, timestamp);
            }
        }

        private void CommutativeRemove(CRDT.Application.Commutative.Register.LWW_RegisterService<TestType> sourceReplica, TestType value, long timestamp, List<CRDT.Application.Commutative.Register.LWW_RegisterService<TestType>> downstreamReplicas)
        {
            sourceReplica.LocalRemove(value, timestamp);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamRemove(value, timestamp);
            }
        }

        #endregion

        #region Convergent

        private Dictionary<Node, CRDT.Application.Convergent.Register.LWW_RegisterService<TestType>> CreateConvergentReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Convergent.Register.LWW_RegisterService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new LWW_RegisterRepository();
                var service = new CRDT.Application.Convergent.Register.LWW_RegisterService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void ConvergentAssign(CRDT.Application.Convergent.Register.LWW_RegisterService<TestType> sourceReplica, TestType value, long timestamp, List<CRDT.Application.Convergent.Register.LWW_RegisterService<TestType>> downstreamReplicas)
        {
            sourceReplica.LocalAssign(value, timestamp);

            var state = sourceReplica.GetValue(value.Id);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamAssign(state.Value, state.Timestamp);
            }
        }

        private void ConvergentRemove(CRDT.Application.Convergent.Register.LWW_RegisterService<TestType> sourceReplica, TestType value, long timestamp, List<CRDT.Application.Convergent.Register.LWW_RegisterService<TestType>> downstreamReplicas)
        {
            sourceReplica.LocalRemove(value, timestamp);

            var state = sourceReplica.GetValue(value.Id);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamRemove(state.Value, state.Timestamp);
            }
        }

        #endregion
    }
}