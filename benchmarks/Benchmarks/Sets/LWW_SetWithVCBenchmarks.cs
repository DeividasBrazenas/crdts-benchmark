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

namespace Benchmarks.Sets
{
    [SimpleJob(RunStrategy.Monitoring, RuntimeMoniker.NetCoreApp50, 1, 5, 10)]
    [MemoryDiagnoser]
    public class LWW_SetWithVCBenchmarks
    {
        private List<Node> _nodes;
        private Dictionary<Node, CRDT.Application.Commutative.Set.LWW_SetWithVCService<TestType>> _commutativeReplicas;
        private Dictionary<Node, CRDT.Application.Convergent.Set.LWW_SetWithVCService<TestType>> _convergentReplicas;
        private List<TestType> _object;

        [Params(100)]
        public int Iterations;

        [IterationSetup]
        public void Setup()
        {
            _nodes = CreateNodes(3);
            _commutativeReplicas = CreateCommutativeReplicas(_nodes);
            _convergentReplicas = CreateConvergentReplicas(_nodes);
            _object = new TestTypeBuilder(new Random()).Build(Guid.NewGuid(), _nodes.Count * Iterations);
        }

        [Benchmark]
        public void Convergent_AddNewValue()
        {
            TestType value;
            CRDT.Application.Convergent.Set.LWW_SetWithVCService<TestType> replica;
            List<CRDT.Application.Convergent.Set.LWW_SetWithVCService<TestType>> downstreamReplicas;
            var clock = new VectorClock(_nodes);

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _convergentReplicas[_nodes[i]];
                downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _object[i * Iterations + j];

                    replica.LocalAdd(value, clock);

                    clock = clock.Increment(_nodes[i]);

                    var (adds, removes) = replica.State;

                    ConvergentDownstreamMerge(adds, removes, downstreamReplicas);
                }
            }
        }

        [Benchmark]
        public void Commutative_AddNewValue()
        {
            TestType value;
            CRDT.Application.Commutative.Set.LWW_SetWithVCService<TestType> replica;
            List<CRDT.Application.Commutative.Set.LWW_SetWithVCService<TestType>> downstreamReplicas;
            var clock = new VectorClock(_nodes);

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _commutativeReplicas[_nodes[i]];
                downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _object[i * Iterations + j];

                    replica.LocalAdd(value, clock);

                    clock = clock.Increment(_nodes[i]);

                    CommutativeDownstreamAdd(value, clock, downstreamReplicas);
                }
            }
        }

        [Benchmark]
        public void Convergent_AddAndRemoveValue()
        {
            TestType value;
            CRDT.Application.Convergent.Set.LWW_SetWithVCService<TestType> replica;
            List<CRDT.Application.Convergent.Set.LWW_SetWithVCService<TestType>> downstreamReplicas;
            var clock = new VectorClock(_nodes);

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _convergentReplicas[_nodes[i]];
                downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _object[i * Iterations + j];

                    replica.LocalAdd(value, clock);

                    clock = clock.Increment(_nodes[i]);

                    var (adds, removes) = replica.State;

                    ConvergentDownstreamMerge(adds, removes, downstreamReplicas);


                    replica.LocalRemove(value, clock);

                    clock = clock.Increment(_nodes[i]);

                    (adds, removes) = replica.State;

                    ConvergentDownstreamMerge(adds, removes, downstreamReplicas);
                }
            }
        }

        [Benchmark]
        public void Commutative_AddAndRemoveValue()
        {
            TestType value;
            CRDT.Application.Commutative.Set.LWW_SetWithVCService<TestType> replica;
            List<CRDT.Application.Commutative.Set.LWW_SetWithVCService<TestType>> downstreamReplicas;
            var clock = new VectorClock(_nodes);

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _commutativeReplicas[_nodes[i]];
                downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < Iterations; j++)
                {
                    value = _object[i * Iterations + j];

                    replica.LocalAdd(value, clock);

                    clock = clock.Increment(_nodes[i]);

                    CommutativeDownstreamAdd(value, clock, downstreamReplicas);


                    replica.LocalRemove(value, clock);

                    clock = clock.Increment(_nodes[i]);

                    CommutativeDownstreamRemove(value, clock, downstreamReplicas);
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

        private Dictionary<Node, CRDT.Application.Commutative.Set.LWW_SetWithVCService<TestType>> CreateCommutativeReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Commutative.Set.LWW_SetWithVCService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new LWW_SetWithVCRepository();
                var service = new CRDT.Application.Commutative.Set.LWW_SetWithVCService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void CommutativeDownstreamAdd(TestType value, VectorClock vectorClock, List<CRDT.Application.Commutative.Set.LWW_SetWithVCService<TestType>> downstreamReplicas)
        {
            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamAdd(value, vectorClock);
            }
        }

        private void CommutativeDownstreamRemove(TestType value, VectorClock vectorClock, List<CRDT.Application.Commutative.Set.LWW_SetWithVCService<TestType>> downstreamReplicas)
        {
            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.DownstreamRemove(value, vectorClock);
            }
        }
        #endregion

        #region Convergent

        private Dictionary<Node, CRDT.Application.Convergent.Set.LWW_SetWithVCService<TestType>> CreateConvergentReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Convergent.Set.LWW_SetWithVCService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new LWW_SetWithVCRepository();
                var service = new CRDT.Application.Convergent.Set.LWW_SetWithVCService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void ConvergentDownstreamMerge(ImmutableHashSet<LWW_SetWithVCElement<TestType>> adds, ImmutableHashSet<LWW_SetWithVCElement<TestType>> removes, List<CRDT.Application.Convergent.Set.LWW_SetWithVCService<TestType>> downstreamReplicas)
        {
            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Merge(adds, removes);
            }
        }

        #endregion
    }
}