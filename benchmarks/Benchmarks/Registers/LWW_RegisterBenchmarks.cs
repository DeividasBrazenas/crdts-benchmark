using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using Benchmarks.Repositories;
using Benchmarks.TestTypes;
using CRDT.Core.Cluster;
using Newtonsoft.Json.Linq;

namespace Benchmarks.Registers
{
    [SimpleJob(RunStrategy.Monitoring, RuntimeMoniker.NetCoreApp50, 1, 10, 100)]
    public class LWW_RegisterBenchmarks
    {
        private List<Node> _nodes;
        private Dictionary<Node, CRDT.Application.Commutative.Register.LWW_RegisterService<TestType>> _commutativeReplicas;
        private Dictionary<Node, CRDT.Application.Convergent.Register.LWW_RegisterService<TestType>> _convergentReplicas;
        private List<TestType> _objects;
        private int _objectsCount;
        private Random _random;

        [IterationSetup]
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

            long ts = 0;

            var firstReplica = _convergentReplicas.First();
            firstReplica.Value.LocalAssign(valueId, value, ts);

            ConvergentDownstreamAssign(firstReplica.Key.Id, firstReplica.Value.GetValue(valueId), ts);

            ts++;

            foreach (var replica in _convergentReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    value = _objects[_random.Next(_objectsCount)];

                    replica.Value.LocalAssign(valueId, value, ts);

                    ConvergentDownstreamAssign(replica.Key.Id, replica.Value.GetValue(valueId), ts);

                    ts++;
                }
            }
        }

        [Benchmark]
        public void Commutative_AssignNewValue()
        {
            var value = _objects[0];
            var valueId = value.Id;

            long ts = 0;

            var firstReplica = _commutativeReplicas.First();
            firstReplica.Value.LocalAssign(valueId, JToken.FromObject(value), ts);

            CommutativeDownstreamAssign(firstReplica.Key.Id, valueId, JToken.FromObject(firstReplica.Value.GetValue(valueId)), ts);

            ts++;

            foreach (var replica in _commutativeReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    value = _objects[_random.Next(_objectsCount)];

                    replica.Value.LocalAssign(valueId, JToken.FromObject(value), ts);

                    CommutativeDownstreamAssign(replica.Key.Id, valueId, JToken.FromObject(replica.Value.GetValue(valueId)), ts);

                    ts++;
                }
            }
        }

        [Benchmark]
        public void Convergent_UpdateSingleField()
        {
            var value = _objects[0];
            var valueId = value.Id;

            long ts = 0;

            var firstReplica = _convergentReplicas.First();
            firstReplica.Value.LocalAssign(valueId, value, ts);

            ConvergentDownstreamAssign(firstReplica.Key.Id, firstReplica.Value.GetValue(valueId), ts);

            ts++;

            foreach (var replica in _convergentReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    value.StringValue = Guid.NewGuid().ToString();

                    replica.Value.LocalAssign(valueId, value, ts);

                    ConvergentDownstreamAssign(replica.Key.Id, replica.Value.GetValue(valueId), ts);

                    ts++;
                }
            }
        }

        [Benchmark]
        public void Commutative_UpdateSingleField()
        {
            var value = _objects[0];
            var valueId = value.Id;

            long ts = 0;

            var firstReplica = _commutativeReplicas.First();
            firstReplica.Value.LocalAssign(valueId, JToken.FromObject(value), ts);

            CommutativeDownstreamAssign(firstReplica.Key.Id, valueId, JToken.FromObject(firstReplica.Value.GetValue(valueId)), ts);

            ts++;

            foreach (var replica in _commutativeReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    value.StringValue = Guid.NewGuid().ToString();

                    var jToken = JToken.Parse($"{{\"StringValue\":\"{value.StringValue}\"}}");

                    replica.Value.LocalAssign(valueId, jToken, ts);

                    CommutativeDownstreamAssign(replica.Key.Id, valueId, jToken, ts);

                    ts++;
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

        private void CommutativeDownstreamAssign(Guid senderId, Guid objectId, JToken value, long timestamp)
        {
            var downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.DownstreamAssign(objectId, value, timestamp);
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

        private void ConvergentDownstreamAssign(Guid senderId, TestType state, long timestamp)
        {
            var downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.DownstreamAssign(senderId, state, timestamp);
            }
        }

        #endregion
    }
}