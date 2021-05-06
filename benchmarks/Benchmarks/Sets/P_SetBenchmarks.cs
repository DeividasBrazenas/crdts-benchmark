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

namespace Benchmarks.Sets
{
    [SimpleJob(RunStrategy.Monitoring, RuntimeMoniker.NetCoreApp50, 1, 5, 10)]
    [MemoryDiagnoser]
    public class P_SetBenchmarks
    {
        private List<Node> _nodes;
        private Dictionary<Node, CRDT.Application.Commutative.Set.P_SetService<TestType>> _commutativeReplicas;
        private Dictionary<Node, CRDT.Application.Convergent.Set.P_SetService<TestType>> _convergentReplicas;
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
        public void Convergent_AddNewValue()
        {
            TestType value;

            foreach (var replica in _convergentReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    value = _objects[_random.Next(_objectsCount)];

                    replica.Value.LocalAdd(value);

                    var (adds, removes) = replica.Value.State;

                    ConvergentDownstreamMerge(replica.Key.Id, adds, removes);
                }
            }
        }

        [Benchmark]
        public void Commutative_AddNewValue()
        {
            TestType value;

            foreach (var replica in _commutativeReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    value = _objects[_random.Next(_objectsCount)];

                    replica.Value.LocalAdd(value);

                    CommutativeDownstreamAdd(replica.Key.Id, value);
                }
            }
        }

        [Benchmark]
        public void Convergent_AddAndRemoveValue()
        {
            TestType value;

            foreach (var replica in _convergentReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    value = _objects[_random.Next(_objectsCount)];

                    replica.Value.LocalAdd(value);

                    var (adds, removes) = replica.Value.State;

                    ConvergentDownstreamMerge(replica.Key.Id, adds, removes);


                    replica.Value.LocalRemove(value);

                    (adds, removes) = replica.Value.State;

                    ConvergentDownstreamMerge(replica.Key.Id, adds, removes);
                }
            }
        }

        [Benchmark]
        public void Commutative_AddAndRemoveValue()
        {
            TestType value;

            foreach (var replica in _commutativeReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    value = _objects[_random.Next(_objectsCount)];

                    replica.Value.LocalAdd(value);

                    CommutativeDownstreamAdd(replica.Key.Id, value);


                    replica.Value.LocalRemove(value);

                    CommutativeDownstreamRemove(replica.Key.Id, value);
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

        private Dictionary<Node, CRDT.Application.Commutative.Set.P_SetService<TestType>> CreateCommutativeReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Commutative.Set.P_SetService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new P_SetRepository();
                var service = new CRDT.Application.Commutative.Set.P_SetService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void CommutativeDownstreamAdd(Guid senderId, TestType value)
        {
            var downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.DownstreamAdd(value);
            }
        }

        private void CommutativeDownstreamRemove(Guid senderId, TestType value)
        {
            var downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.DownstreamRemove(value);
            }
        }
        #endregion

        #region Convergent

        private Dictionary<Node, CRDT.Application.Convergent.Set.P_SetService<TestType>> CreateConvergentReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Convergent.Set.P_SetService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new P_SetRepository();
                var service = new CRDT.Application.Convergent.Set.P_SetService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void ConvergentDownstreamMerge(Guid senderId, IEnumerable<TestType> adds, IEnumerable<TestType> removes)
        {
            var downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.Merge(adds, removes);
            }
        }

        #endregion
    }
}