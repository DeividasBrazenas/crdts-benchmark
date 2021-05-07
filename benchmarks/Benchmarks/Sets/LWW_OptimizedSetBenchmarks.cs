using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
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
        private Dictionary<Node, CRDT.Application.Commutative.Set.LWW_OptimizedSetService<TestType>> _commutativeReplicas;
        private Dictionary<Node, CRDT.Application.Convergent.Set.LWW_OptimizedSetService<TestType>> _convergentReplicas;
        private List<TestType> _objects;

        [Params(100)]
        public int Iterations;

        [IterationSetup]
        public void Setup()
        {
            _nodes = CreateNodes(3);
            _commutativeReplicas = CreateCommutativeReplicas(_nodes);
            _convergentReplicas = CreateConvergentReplicas(_nodes);
            _objects = new TestTypeBuilder(new Random()).Build(Guid.NewGuid(), _nodes.Count * Iterations);
        }

        [Benchmark]
        public void Convergent_AddNewValue()
        {
            TestType value;
            CRDT.Application.Convergent.Set.LWW_OptimizedSetService<TestType> replica;
            List<CRDT.Application.Convergent.Set.LWW_OptimizedSetService<TestType>> downstreamReplicas;
            var t = 0;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _convergentReplicas[_nodes[i]];
                downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, t++);

                    ConvergentDownstreamMerge(replica.State, downstreamReplicas);
                }
            }
        }

        [Benchmark]
        public void Commutative_AddNewValue()
        {
            TestType value;
            CRDT.Application.Commutative.Set.LWW_OptimizedSetService<TestType> replica;
            List<CRDT.Application.Commutative.Set.LWW_OptimizedSetService<TestType>> downstreamReplicas;
            var t = 0;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _commutativeReplicas[_nodes[i]];
                downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, t++);

                    CommutativeDownstreamAdd(value, t, downstreamReplicas);
                }
            }
        }

        [Benchmark]
        public void Convergent_AddAndRemoveValue()
        {
            TestType value;
            CRDT.Application.Convergent.Set.LWW_OptimizedSetService<TestType> replica;
            List<CRDT.Application.Convergent.Set.LWW_OptimizedSetService<TestType>> downstreamReplicas;
            var t = 0;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _convergentReplicas[_nodes[i]];
                downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, t++);

                    ConvergentDownstreamMerge(replica.State, downstreamReplicas);

                    replica.LocalRemove(value, t++);

                    ConvergentDownstreamMerge(replica.State, downstreamReplicas);
                }
            }
        }

        [Benchmark]
        public void Commutative_AddAndRemoveValue()
        {
            TestType value;
            CRDT.Application.Commutative.Set.LWW_OptimizedSetService<TestType> replica;
            List<CRDT.Application.Commutative.Set.LWW_OptimizedSetService<TestType>> downstreamReplicas;
            var t = 0;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _commutativeReplicas[_nodes[i]];
                downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, t++);

                    CommutativeDownstreamAdd(value, t, downstreamReplicas);


                    replica.LocalRemove(value, t++);

                    CommutativeDownstreamRemove(value, t, downstreamReplicas);
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

        private void CommutativeDownstreamAdd(TestType value, long timestamp, List<CRDT.Application.Commutative.Set.LWW_OptimizedSetService<TestType>> downstreamReplicas)
        {
            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamAdd(value, timestamp);
            }
        }

        private void CommutativeDownstreamRemove(TestType value, long timestamp, List<CRDT.Application.Commutative.Set.LWW_OptimizedSetService<TestType>> downstreamReplicas)
        {
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

        private void ConvergentDownstreamMerge(ImmutableHashSet<LWW_OptimizedSetElement<TestType>> elements, List<CRDT.Application.Convergent.Set.LWW_OptimizedSetService<TestType>> downstreamReplicas)
        {
            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Merge(elements);
            }
        }

        #endregion
    }
}