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
using CRDT.Core.DistributedTime;
using CRDT.Sets.Entities;
using Newtonsoft.Json.Linq;

namespace Benchmarks.Sets
{
    [SimpleJob(RunStrategy.Monitoring, RuntimeMoniker.NetCoreApp50, 1, 5, 10)]
    [MemoryDiagnoser]
    public class OUR_SetWithVCBenchmarks
    {
        private List<Node> _nodes;
        private Dictionary<Node, CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType>> _commutativeReplicas;
        private Dictionary<Node, CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType>> _convergentReplicas;
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
            CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType> replica;
            List<CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType>> downstreamReplicas;
            var clock = new VectorClock(_nodes);

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _convergentReplicas[_nodes[i]];
                downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, _nodes[i].Id, clock);
                    var (adds, removes) = replica.State;
                    ConvergentDownstreamMerge(adds, removes, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);
                }
            }
        }

        [Benchmark]
        public void Commutative_AddNewValue()
        {
            TestType value;
            CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType> replica;
            List<CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType>> downstreamReplicas;
            var clock = new VectorClock(_nodes);

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _commutativeReplicas[_nodes[i]];
                downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, _nodes[i].Id, clock);
                    CommutativeDownstreamAdd(value, _nodes[i].Id, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);
                }
            }
        }

        [Benchmark]
        public void Convergent_AddAndUpdateValue()
        {
            TestType value;
            CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType> replica;
            List<CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType>> downstreamReplicas;
            var clock = new VectorClock(_nodes);

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _convergentReplicas[_nodes[i]];
                downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, _nodes[i].Id, clock);
                    var (adds, removes) = replica.State;
                    ConvergentDownstreamMerge(adds, removes, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);

                    value = _objects[2 * i * Iterations + j];

                    replica.LocalUpdate(value, _nodes[i].Id, clock);
                    (adds, removes) = replica.State;
                    ConvergentDownstreamMerge(adds, removes, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);
                }
            }
        }

        [Benchmark]
        public void Commutative_AddAndUpdateValue()
        {
            TestType value;
            CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType> replica;
            List<CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType>> downstreamReplicas;
            var clock = new VectorClock(_nodes);

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _commutativeReplicas[_nodes[i]];
                downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, _nodes[i].Id, clock);
                    CommutativeDownstreamAdd(value, _nodes[i].Id, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);

                    value = _objects[2 * i * Iterations + j];

                    replica.LocalUpdate(value, _nodes[i].Id, clock);
                    CommutativeDownstreamUpdate(value, _nodes[i].Id, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);
                }
            }
        }

        [Benchmark]
        public void Convergent_AddAndRemoveValue()
        {
            TestType value;
            CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType> replica;
            List<CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType>> downstreamReplicas;
            var clock = new VectorClock(_nodes);

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _convergentReplicas[_nodes[i]];
                downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, _nodes[i].Id, clock);
                    var (adds, removes) = replica.State;
                    ConvergentDownstreamMerge(adds, removes, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);

                    var observedTags = replica.GetTags(value);
                    replica.LocalRemove(value, observedTags, clock);
                    (adds, removes) = replica.State;
                    ConvergentDownstreamMerge(adds, removes, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);
                }
            }
        }

        [Benchmark]
        public void Commutative_AddAndRemoveValue()
        {
            TestType value;
            CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType> replica;
            List<CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType>> downstreamReplicas;
            var clock = new VectorClock(_nodes);

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _commutativeReplicas[_nodes[i]];
                downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, _nodes[i].Id, clock);
                    CommutativeDownstreamAdd(value, _nodes[i].Id, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);

                    var observedTags = replica.GetTags(value);
                    replica.LocalRemove(value, observedTags, clock);
                    CommutativeDownstreamRemove(value, observedTags, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);
                }
            }
        }

        [Benchmark]
        public void Convergent_AddUpdateAndRemoveValue()
        {
            TestType value;
            CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType> replica;
            List<CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType>> downstreamReplicas;
            var clock = new VectorClock(_nodes);

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _convergentReplicas[_nodes[i]];
                downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, _nodes[i].Id, clock);
                    var (adds, removes) = replica.State;
                    ConvergentDownstreamMerge(adds, removes, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);

                    value = _objects[2 * i * Iterations + j];

                    replica.LocalUpdate(value, _nodes[i].Id, clock);
                    (adds, removes) = replica.State;
                    ConvergentDownstreamMerge(adds, removes, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);

                    var observedTags = replica.GetTags(value);
                    replica.LocalRemove(value, observedTags, clock);
                    (adds, removes) = replica.State;
                    ConvergentDownstreamMerge(adds, removes, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);
                }
            }
        }

        [Benchmark]
        public void Commutative_AddUpdateAndRemoveValue()
        {
            TestType value;
            CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType> replica;
            List<CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType>> downstreamReplicas;
            var clock = new VectorClock(_nodes);

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _commutativeReplicas[_nodes[i]];
                downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _objects[i * Iterations + j];

                    replica.LocalAdd(value, _nodes[i].Id, clock);
                    CommutativeDownstreamAdd(value, _nodes[i].Id, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);

                    value = _objects[2 * i * Iterations + j];

                    replica.LocalUpdate(value, _nodes[i].Id, clock);
                    CommutativeDownstreamUpdate(value, _nodes[i].Id, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);

                    var observedTags = replica.GetTags(value);
                    replica.LocalRemove(value, observedTags, clock);
                    CommutativeDownstreamRemove(value, observedTags, clock, downstreamReplicas);
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

        private void CommutativeDownstreamAdd(TestType value, Guid tag, VectorClock clock, List<CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType>> downstreamReplicas)
        {
            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamAdd(value, tag, clock);
            }
        }

        private void CommutativeDownstreamUpdate(TestType value, Guid tag, VectorClock clock, List<CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType>> downstreamReplicas)
        {
            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamUpdate(value, tag, clock);
            }
        }

        private void CommutativeDownstreamRemove(TestType value, List<Guid> tags, VectorClock clock, List<CRDT.Application.Commutative.Set.OUR_SetWithVCService<TestType>> downstreamReplicas)
        {
            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamRemove(value, tags, clock);
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

        private void ConvergentDownstreamMerge(ImmutableHashSet<OUR_SetWithVCElement<TestType>> adds, ImmutableHashSet<OUR_SetWithVCElement<TestType>> removes, List<CRDT.Application.Convergent.Set.OUR_SetWithVCService<TestType>> downstreamReplicas)
        {
            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Merge(adds, removes);
            }
        }

        #endregion
    }
}