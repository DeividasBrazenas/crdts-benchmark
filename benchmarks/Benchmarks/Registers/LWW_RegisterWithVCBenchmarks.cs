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
        private int _objectsCount;
        private Random _random;

        [GlobalSetup]
        public void Setup()
        {
            _nodes = CreateNodes(3);
            _commutativeReplicas = CreateCommutativeReplicas(_nodes);
            _convergentReplicas = CreateConvergentReplicas(_nodes);
            _random = new Random();
            _objectsCount = 1000;
            _objects = new TestTypeBuilder(_random).Build(Guid.NewGuid(), _objectsCount);
        }

        [Benchmark]
        public void Convergent_AssignNewValue()
        {
            var value = _objects[0];
            var valueId = value.Id;

            var clock = new VectorClock(_nodes);

            var firstReplica = _convergentReplicas.First();
            firstReplica.Value.LocalAssign(valueId, value, clock);

            ConvergentDownstreamAssign(firstReplica.Key.Id, firstReplica.Value.GetValue(valueId), clock);

            clock = clock.Increment(firstReplica.Key);

            foreach (var replica in _convergentReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    value = _objects[_random.Next(_objectsCount)];

                    replica.Value.LocalAssign(valueId, value, clock);

                    ConvergentDownstreamAssign(replica.Key.Id, replica.Value.GetValue(valueId), clock);

                    clock = clock.Increment(replica.Key);
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

            CommutativeDownstreamAssign(firstReplica.Key.Id, valueId, JToken.FromObject(firstReplica.Value.GetValue(valueId)), clock);

            clock = clock.Increment(firstReplica.Key);

            foreach (var replica in _commutativeReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    value = _objects[_random.Next(_objectsCount)];

                    replica.Value.LocalAssign(valueId, JToken.FromObject(value), clock);

                    CommutativeDownstreamAssign(replica.Key.Id, valueId, JToken.FromObject(replica.Value.GetValue(valueId)), clock);

                    clock = clock.Increment(replica.Key);
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

            ConvergentDownstreamAssign(firstReplica.Key.Id, firstReplica.Value.GetValue(valueId), clock);

            clock = clock.Increment(firstReplica.Key);

            foreach (var replica in _convergentReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    value.StringValue = Guid.NewGuid().ToString();

                    replica.Value.LocalAssign(valueId, value, clock);

                    ConvergentDownstreamAssign(replica.Key.Id, replica.Value.GetValue(valueId), clock);

                    clock = clock.Increment(replica.Key);
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

            CommutativeDownstreamAssign(firstReplica.Key.Id, valueId, JToken.FromObject(firstReplica.Value.GetValue(valueId)), clock);

            clock = clock.Increment(firstReplica.Key);

            foreach (var replica in _commutativeReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    value.StringValue = Guid.NewGuid().ToString();

                    var jToken = JToken.Parse($"{{\"StringValue\":\"{value.StringValue}\"}}");

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

        private void CommutativeDownstreamAssign(Guid senderId, Guid objectId, JToken value, VectorClock clock)
        {
            var downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.DownstreamAssign(objectId, value, clock);
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

        private void ConvergentDownstreamAssign(Guid senderId, TestType state, VectorClock clock)
        {
            var downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.DownstreamAssign(state.Id, state, clock);
            }
        }

        #endregion
    }
}