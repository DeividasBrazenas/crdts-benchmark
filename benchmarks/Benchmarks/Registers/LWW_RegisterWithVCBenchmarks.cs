using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using Benchmarks.Repositories;
using Benchmarks.TestTypes;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using Newtonsoft.Json.Linq;

namespace Benchmarks.Registers
{
    [SimpleJob(RunStrategy.Monitoring, RuntimeMoniker.NetCoreApp50, 1, 10, 100)]
    public class LWW_RegisterWithVCBenchmarks
    {
        private List<Node> _nodes;
        private Dictionary<Node, CRDT.Application.Commutative.Register.LWW_RegisterWithVCService<TestType>> _commutativeReplicas;
        private Dictionary<Node, CRDT.Application.Convergent.Register.LWW_RegisterWithVCService<TestType>> _convergentReplicas;
        private List<TestType> _objects;

        [Params(100)]
        private int Iterations { get; }

        [IterationSetup]
        public void Setup()
        {
            _nodes = CreateNodes(3);
            _commutativeReplicas = CreateCommutativeReplicas(_nodes);
            _convergentReplicas = CreateConvergentReplicas(_nodes);
            _objects = new TestTypeBuilder(new Random()).Build(Guid.NewGuid(), _nodes.Count * Iterations);
        }

        [Benchmark]
        public void Convergent_AssignNewValue()
        {
            var value = _objects[0];
            var valueId = value.Id;

            var clock = new VectorClock(_nodes);

            var firstReplica = _convergentReplicas.First();
            firstReplica.Value.LocalAssign(valueId, value, clock);

            ConvergentDownstreamAssign(firstReplica.Value.GetValue(valueId), clock, _convergentReplicas.Where(r => !Equals(r, firstReplica)).Select(v => v.Value).ToList());

            clock = clock.Increment(firstReplica.Key);

            CRDT.Application.Convergent.Register.LWW_RegisterWithVCService<TestType> replica;
            List<CRDT.Application.Convergent.Register.LWW_RegisterWithVCService<TestType>> downstreamReplicas;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _convergentReplicas[_nodes[i]];
                downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAssign(valueId, value, clock);

                    ConvergentDownstreamAssign(replica.GetValue(valueId), clock, downstreamReplicas);

                    clock = clock.Increment(_nodes[i]);
                }
            }
        }

        [Benchmark]
        public void Commutative_AssignNewValue()
        {
            var value = _objects[0];
            var valueId = value.Id;

            var clock = new VectorClock(_nodes);

            var firstReplica = _commutativeReplicas.First();
            firstReplica.Value.LocalAssign(valueId, JToken.FromObject(value), clock);

            CommutativeDownstreamAssign(valueId, JToken.FromObject(firstReplica.Value.GetValue(valueId)), clock, _commutativeReplicas.Where(r => !Equals(r, firstReplica)).Select(v => v.Value).ToList());

            clock = clock.Increment(firstReplica.Key);

            CRDT.Application.Commutative.Register.LWW_RegisterWithVCService<TestType> replica;
            List<CRDT.Application.Commutative.Register.LWW_RegisterWithVCService<TestType>> downstreamReplicas;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _commutativeReplicas[_nodes[i]];
                downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAssign(valueId, JToken.FromObject(value), clock);

                    CommutativeDownstreamAssign(valueId, JToken.FromObject(replica.GetValue(valueId)), clock, downstreamReplicas);

                    clock = clock.Increment(_nodes[i]);
                }
            }
        }

        [Benchmark]
        public void Convergent_UpdateSingleField()
        {
            var value = _objects[0];
            var valueId = value.Id;

            var clock = new VectorClock(_nodes);

            var firstReplica = _convergentReplicas.First();
            firstReplica.Value.LocalAssign(valueId, value, clock);

            ConvergentDownstreamAssign(firstReplica.Value.GetValue(valueId), clock, _convergentReplicas.Where(r => !Equals(r, firstReplica)).Select(v => v.Value).ToList());

            clock = clock.Increment(firstReplica.Key);

            CRDT.Application.Convergent.Register.LWW_RegisterWithVCService<TestType> replica;
            List<CRDT.Application.Convergent.Register.LWW_RegisterWithVCService<TestType>> downstreamReplicas;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _convergentReplicas[_nodes[i]];
                downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value.StringValue = Guid.NewGuid().ToString();

                    replica.LocalAssign(valueId, value, clock);

                    ConvergentDownstreamAssign(replica.GetValue(valueId), clock, downstreamReplicas);

                    clock = clock.Increment(_nodes[i]);
                }
            }
        }

        [Benchmark]
        public void Commutative_UpdateSingleField()
        {
            var value = _objects[0];
            var valueId = value.Id;

            var clock = new VectorClock(_nodes);

            var firstReplica = _commutativeReplicas.First();
            firstReplica.Value.LocalAssign(valueId, JToken.FromObject(value), clock);

            CommutativeDownstreamAssign(valueId, JToken.FromObject(firstReplica.Value.GetValue(valueId)), clock, _commutativeReplicas.Where(r => !Equals(r, firstReplica)).Select(v => v.Value).ToList());

            clock = clock.Increment(firstReplica.Key);

            CRDT.Application.Commutative.Register.LWW_RegisterWithVCService<TestType> replica;
            List<CRDT.Application.Commutative.Register.LWW_RegisterWithVCService<TestType>> downstreamReplicas;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _commutativeReplicas[_nodes[i]];
                downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value.StringValue = Guid.NewGuid().ToString();

                    var jToken = JToken.Parse($"{{\"StringValue\":\"{value.StringValue}\"}}");

                    replica.LocalAssign(valueId, jToken, clock);

                    CommutativeDownstreamAssign(valueId, jToken, clock, downstreamReplicas);

                    clock = clock.Increment(_nodes[i]);
                }
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

        private void CommutativeDownstreamAssign(Guid objectId, JToken value, VectorClock clock, List<CRDT.Application.Commutative.Register.LWW_RegisterWithVCService<TestType>> downstreamReplicas)
        {
            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamAssign(objectId, value, clock);
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

        private void ConvergentDownstreamAssign(TestType state, VectorClock clock, List<CRDT.Application.Convergent.Register.LWW_RegisterWithVCService<TestType>> downstreamReplicas)
        {
            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamAssign(state.Id, state, clock);
            }
        }

        #endregion
    }
}