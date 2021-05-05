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
        private TestTypeBuilder _builder;

        [GlobalSetup]
        public void Setup()
        {
            _nodes = CreateNodes(3);
            _commutativeReplicas = CreateCommutativeReplicas(_nodes);
            _convergentReplicas = CreateConvergentReplicas(_nodes);
            _builder = new TestTypeBuilder(new Random());
        }

        [Benchmark]
        public void Convergent_Assign_NewValue()
        {
            var initialValue = _builder.Build();
            var valueId = initialValue.Id;

            var clock = new VectorClock(_nodes);

            var firstReplica = _convergentReplicas.First();
            firstReplica.Value.LocalAssign(valueId, initialValue, clock);

            ConvergentDownstreamAssign(firstReplica.Key.Id, firstReplica.Value.GetValue(valueId), clock);

            clock = clock.Increment(firstReplica.Key);

            foreach (var replica in _convergentReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    initialValue = _builder.Build(valueId);

                    replica.Value.LocalAssign(valueId, initialValue, clock);

                    ConvergentDownstreamAssign(replica.Key.Id, replica.Value.GetValue(valueId), clock);

                    clock = clock.Increment(replica.Key);
                }
            }
        }

        [Benchmark]
        public void Commutative_Assign_NewValue()
        {
            var initialValue = _builder.Build();
            var valueId = initialValue.Id;

            var clock = new VectorClock(_nodes);

            var firstReplica = _commutativeReplicas.First();
            firstReplica.Value.LocalAssign(valueId, JToken.FromObject(initialValue), clock);

            CommutativeDownstreamAssign(firstReplica.Key.Id, valueId, JToken.FromObject(firstReplica.Value.GetValue(valueId)), clock);

            clock = clock.Increment(firstReplica.Key);

            foreach (var replica in _commutativeReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    initialValue = _builder.Build(valueId);

                    replica.Value.LocalAssign(valueId, JToken.FromObject(initialValue), clock);

                    CommutativeDownstreamAssign(replica.Key.Id, valueId, JToken.FromObject(replica.Value.GetValue(valueId)), clock);

                    clock = clock.Increment(replica.Key);
                }
            }
        }

        [Benchmark]
        public void Convergent_Assign_UpdateSingleField()
        {
            var initialValue = _builder.Build();
            var valueId = initialValue.Id;

            var clock = new VectorClock(_nodes);

            var firstReplica = _convergentReplicas.First();
            firstReplica.Value.LocalAssign(valueId, initialValue, clock);

            ConvergentDownstreamAssign(firstReplica.Key.Id, firstReplica.Value.GetValue(valueId), clock);

            clock = clock.Increment(firstReplica.Key);

            foreach (var replica in _convergentReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    initialValue.StringValue = Guid.NewGuid().ToString();

                    replica.Value.LocalAssign(valueId, initialValue, clock);

                    ConvergentDownstreamAssign(replica.Key.Id, replica.Value.GetValue(valueId), clock);

                    clock = clock.Increment(replica.Key);
                }
            }
        }

        [Benchmark]
        public void Commutative_Assign_UpdateSingleField()
        {
            var initialValue = _builder.Build();
            var valueId = initialValue.Id;

            var clock = new VectorClock(_nodes);

            var firstReplica = _commutativeReplicas.First();
            firstReplica.Value.LocalAssign(valueId, JToken.FromObject(initialValue), clock);

            CommutativeDownstreamAssign(firstReplica.Key.Id, valueId, JToken.FromObject(firstReplica.Value.GetValue(valueId)), clock);

            clock = clock.Increment(firstReplica.Key);

            foreach (var replica in _commutativeReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    initialValue.StringValue = Guid.NewGuid().ToString();

                    var jToken = JToken.Parse($"{{\"StringValue\":\"{initialValue.StringValue}\"}}");

                    replica.Value.LocalAssign(valueId, jToken, clock);

                    CommutativeDownstreamAssign(replica.Key.Id, valueId, jToken, clock);

                    clock = clock.Increment(replica.Key);
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

        private bool CommutativeDownstreamAssign(Guid senderId, Guid objectId, JToken value, VectorClock clock)
        {
            var downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.DownstreamAssign(objectId, value, clock);
            }

            return true;
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

        private bool ConvergentDownstreamAssign(Guid senderId, TestType state, VectorClock clock)
        {
            var downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.DownstreamAssign(state.Id, state, clock);
            }

            return true;
        }

        #endregion
    }
}