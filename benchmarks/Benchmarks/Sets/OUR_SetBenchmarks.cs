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
    public class OUR_SetBenchmarks
    {
        private List<Node> _nodes;
        private Dictionary<Node, CRDT.Application.Commutative.Set.OUR_SetService<TestType>> _commutativeReplicas;
        private Dictionary<Node, CRDT.Application.Convergent.Set.OUR_SetService<TestType>> _convergentReplicas;
        private List<TestType> _objects;

        [Params(100)]
        public int Iterations;

        [IterationSetup]
        public void Setup()
        {
            _nodes = CreateNodes(3);
            _commutativeReplicas = CreateCommutativeReplicas(_nodes);
            _convergentReplicas = CreateConvergentReplicas(_nodes);
            _objects = new TestTypeBuilder(new Random()).Build(Guid.NewGuid(), _nodes.Count * Iterations * 2);
        }

        [Benchmark]
        public void Convergent_AddNewValue()
        {
            TestType value;
            CRDT.Application.Convergent.Set.OUR_SetService<TestType> replica;
            List<CRDT.Application.Convergent.Set.OUR_SetService<TestType>> downstreamReplicas;
            var ts = 0;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _convergentReplicas[_nodes[i]];
                downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, _nodes[i].Id, ts++);

                    var (adds, removes) = replica.State;

                    ConvergentDownstreamMerge(adds, removes, downstreamReplicas);
                }
            }
        }

        [Benchmark]
        public void Commutative_AddNewValue()
        {
            TestType value;
            CRDT.Application.Commutative.Set.OUR_SetService<TestType> replica;
            List<CRDT.Application.Commutative.Set.OUR_SetService<TestType>> downstreamReplicas;
            var ts = 0;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _commutativeReplicas[_nodes[i]];
                downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, _nodes[i].Id, ts++);

                    CommutativeDownstreamAdd(value, _nodes[i].Id, ts, downstreamReplicas);
                }
            }
        }

        [Benchmark]
        public void Convergent_AddAndUpdateValue()
        {
            TestType value;
            CRDT.Application.Convergent.Set.OUR_SetService<TestType> replica;
            List<CRDT.Application.Convergent.Set.OUR_SetService<TestType>> downstreamReplicas;
            var ts = 0;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _convergentReplicas[_nodes[i]];
                downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, _nodes[i].Id, ts++);
                    var (adds, removes) = replica.State;
                    ConvergentDownstreamMerge(adds, removes, downstreamReplicas);

                    value = _objects[2 * i * Iterations + j];

                    replica.LocalUpdate(value, _nodes[i].Id, ts++);
                    (adds, removes) = replica.State;
                    ConvergentDownstreamMerge(adds, removes, downstreamReplicas);
                }
            }
        }

        [Benchmark]
        public void Commutative_AddAndUpdateValue()
        {
            TestType value;
            CRDT.Application.Commutative.Set.OUR_SetService<TestType> replica;
            List<CRDT.Application.Commutative.Set.OUR_SetService<TestType>> downstreamReplicas;
            var ts = 0;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _commutativeReplicas[_nodes[i]];
                downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, _nodes[i].Id, ts++);
                    CommutativeDownstreamAdd(value, _nodes[i].Id, ts, downstreamReplicas);

                    value = _objects[2 * i * Iterations + j];

                    replica.LocalUpdate(value, _nodes[i].Id, ts++);
                    CommutativeDownstreamUpdate(value, _nodes[i].Id, ts, downstreamReplicas);
                }
            }
        }

        [Benchmark]
        public void Convergent_AddAndRemoveValue()
        {
            TestType value;
            CRDT.Application.Convergent.Set.OUR_SetService<TestType> replica;
            List<CRDT.Application.Convergent.Set.OUR_SetService<TestType>> downstreamReplicas;
            var ts = 0;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _convergentReplicas[_nodes[i]];
                downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, _nodes[i].Id, ts++);
                    var (adds, removes) = replica.State;
                    ConvergentDownstreamMerge(adds, removes, downstreamReplicas);

                    var observedTags = replica.GetTags(value);
                    replica.LocalRemove(value, observedTags, ts++);
                    (adds, removes) = replica.State;
                    ConvergentDownstreamMerge(adds, removes, downstreamReplicas);
                }
            }
        }

        [Benchmark]
        public void Commutative_AddAndRemoveValue()
        {
            TestType value;
            CRDT.Application.Commutative.Set.OUR_SetService<TestType> replica;
            List<CRDT.Application.Commutative.Set.OUR_SetService<TestType>> downstreamReplicas;
            var ts = 0;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _commutativeReplicas[_nodes[i]];
                downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, _nodes[i].Id, ts++);
                    CommutativeDownstreamAdd(value, _nodes[i].Id, ts, downstreamReplicas);

                    var observedTags = replica.GetTags(value);
                    replica.LocalRemove(value, observedTags, ts++);
                    CommutativeDownstreamRemove(value, observedTags, ts, downstreamReplicas);
                }
            }
        }

        [Benchmark]
        public void Convergent_AddUpdateAndRemoveValue()
        {
            TestType value;
            CRDT.Application.Convergent.Set.OUR_SetService<TestType> replica;
            List<CRDT.Application.Convergent.Set.OUR_SetService<TestType>> downstreamReplicas;
            var ts = 0;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _convergentReplicas[_nodes[i]];
                downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, _nodes[i].Id, ts++);
                    var (adds, removes) = replica.State;
                    ConvergentDownstreamMerge(adds, removes, downstreamReplicas);

                    value = _objects[2 * i * Iterations + j];

                    replica.LocalUpdate(value, _nodes[i].Id, ts++);
                    (adds, removes) = replica.State;
                    ConvergentDownstreamMerge(adds, removes, downstreamReplicas);

                    var observedTags = replica.GetTags(value);
                    replica.LocalRemove(value, observedTags, ts++);
                    (adds, removes) = replica.State;
                    ConvergentDownstreamMerge(adds, removes, downstreamReplicas);
                }
            }
        }

        [Benchmark]
        public void Commutative_AddUpdateAndRemoveValue()
        {
            TestType value;
            CRDT.Application.Commutative.Set.OUR_SetService<TestType> replica;
            List<CRDT.Application.Commutative.Set.OUR_SetService<TestType>> downstreamReplicas;
            var ts = 0;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _commutativeReplicas[_nodes[i]];
                downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, _nodes[i].Id, ts++);
                    CommutativeDownstreamAdd(value, _nodes[i].Id, ts, downstreamReplicas);

                    value = _objects[2 * i * Iterations + j];

                    replica.LocalUpdate(value, _nodes[i].Id, ts++);
                    CommutativeDownstreamUpdate(value, _nodes[i].Id, ts, downstreamReplicas);

                    var observedTags = replica.GetTags(value);
                    replica.LocalRemove(value, observedTags, ts++);
                    CommutativeDownstreamRemove(value, observedTags, ts, downstreamReplicas);
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

        private Dictionary<Node, CRDT.Application.Commutative.Set.OUR_SetService<TestType>> CreateCommutativeReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Commutative.Set.OUR_SetService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new OUR_SetRepository();
                var service = new CRDT.Application.Commutative.Set.OUR_SetService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void CommutativeDownstreamAdd(TestType value, Guid tag, long timestamp, List<CRDT.Application.Commutative.Set.OUR_SetService<TestType>> downstreamReplicas)
        {
            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamAdd(value, tag, timestamp);
            }
        }

        private void CommutativeDownstreamUpdate(TestType value, Guid tag, long timestamp, List<CRDT.Application.Commutative.Set.OUR_SetService<TestType>> downstreamReplicas)
        {
            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamUpdate(value, tag, timestamp);
            }
        }

        private void CommutativeDownstreamRemove(TestType value, List<Guid> tags, long timestamp, List<CRDT.Application.Commutative.Set.OUR_SetService<TestType>> downstreamReplicas)
        {
            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamRemove(value, tags, timestamp);
            }
        }
        #endregion

        #region Convergent

        private Dictionary<Node, CRDT.Application.Convergent.Set.OUR_SetService<TestType>> CreateConvergentReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Convergent.Set.OUR_SetService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new OUR_SetRepository();
                var service = new CRDT.Application.Convergent.Set.OUR_SetService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void ConvergentDownstreamMerge(ImmutableHashSet<OUR_SetElement<TestType>> adds, ImmutableHashSet<OUR_SetElement<TestType>> removes, List<CRDT.Application.Convergent.Set.OUR_SetService<TestType>> downstreamReplicas)
        {
            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Merge(adds, removes);
            }
        }

        #endregion
    }
}