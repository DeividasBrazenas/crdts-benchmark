using System;
using System.Collections.Generic;
using System.Linq;
using Benchmarks.TestTypes;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;

namespace Benchmarks.Framework
{
    public class CRDT_Set_Benchmarker<TCRDT>
    {
        private int _iterations;
        private readonly List<Node> _nodes;
        private readonly Dictionary<Node, TCRDT> _replicas;
        private readonly List<TestType> _addObjects;
        private readonly List<TestType> _updateObjects;

        public Action<TCRDT, TestType, List<TCRDT>> Add { get; set; }
        public Action<TCRDT, TestType, List<TCRDT>> Remove { get; set; }

        public Action<TCRDT, TestType, long, List<TCRDT>> AddWithTimestamp { get; set; }
        public Action<TCRDT, TestType, long, List<TCRDT>> UpdateWithTimestamp { get; set; }
        public Action<TCRDT, TestType, long, List<TCRDT>> RemoveWithTimestamp { get; set; }

        public Action<TCRDT, TestType, VectorClock, List<TCRDT>> AddWithVectorClock { get; set; }
        public Action<TCRDT, TestType, VectorClock, List<TCRDT>> UpdateWithVectorClock { get; set; }
        public Action<TCRDT, TestType, VectorClock, List<TCRDT>> RemoveWithVectorClock { get; set; }

        public CRDT_Set_Benchmarker(int iterations, List<Node> nodes, Dictionary<Node, TCRDT> replicas)
        {
            _iterations = iterations;
            _nodes = nodes;
            _replicas = replicas;
            var objectIds = GenerateGuids(iterations * _nodes.Count);
            var random = new Random();
            _addObjects = GenerateObjects(random, objectIds);
            _updateObjects = GenerateObjects(random, objectIds);
        }

        #region WithoutTime

        public void Benchmark_Add()
        {
            TestType value;
            TCRDT replica;
            List<TCRDT> downstreamReplicas;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _replicas[_nodes[i]];
                downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < _iterations; j++)
                {
                    // Add
                    value = _addObjects[i * _iterations + j];
                    Add(replica, value, downstreamReplicas);
                }
            }
        }
        
        public void Benchmark_AddAndRemove()
        {
            TestType value;
            TCRDT replica;
            List<TCRDT> downstreamReplicas;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _replicas[_nodes[i]];
                downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < _iterations; j++)
                {
                    // Add
                    value = _addObjects[i * _iterations + j];
                    Add(replica, value, downstreamReplicas);

                    // Remove
                    Remove(replica, value, downstreamReplicas);
                }
            }
        }
        
        #endregion

        #region WithTimestamp

        public void Benchmark_Add_WithTimestamp()
        {
            TestType value;
            TCRDT replica;
            List<TCRDT> downstreamReplicas;

            var ts = 1;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _replicas[_nodes[i]];
                downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < _iterations; j++)
                {
                    // Add
                    value = _addObjects[i * _iterations + j];
                    AddWithTimestamp(replica, value, ts, downstreamReplicas);
                    ts++;
                }
            }
        }

        public void Benchmark_AddAndUpdate_WithTimestamp()
        {
            TestType value;
            TCRDT replica;
            List<TCRDT> downstreamReplicas;

            var ts = 1;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _replicas[_nodes[i]];
                downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < _iterations; j++)
                {
                    // Add
                    value = _addObjects[i * _iterations + j];
                    AddWithTimestamp(replica, value, ts, downstreamReplicas);
                    ts++;

                    // Update
                    value = _updateObjects[i * _iterations + j];
                    UpdateWithTimestamp(replica, value, ts, downstreamReplicas);
                    ts++;
                }
            }
        }

        public void Benchmark_AddAndRemove_WithTimestamp()
        {
            TestType value;
            TCRDT replica;
            List<TCRDT> downstreamReplicas;

            var ts = 1;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _replicas[_nodes[i]];
                downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < _iterations; j++)
                {
                    // Add
                    value = _addObjects[i * _iterations + j];
                    AddWithTimestamp(replica, value, ts, downstreamReplicas);
                    ts++;

                    // Remove
                    RemoveWithTimestamp(replica, value, ts, downstreamReplicas);
                    ts++;
                }
            }
        }

        public void Benchmark_AddUpdateAndRemove_WithTimestamp()
        {
            TestType value;
            TCRDT replica;
            List<TCRDT> downstreamReplicas;

            var ts = 1;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _replicas[_nodes[i]];
                downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < _iterations; j++)
                {
                    // Add
                    value = _addObjects[i * _iterations + j];
                    AddWithTimestamp(replica, value, ts, downstreamReplicas);
                    ts++;

                    // Update
                    value = _updateObjects[i * _iterations + j];
                    UpdateWithTimestamp(replica, value, ts, downstreamReplicas);
                    ts++;

                    // Remove
                    RemoveWithTimestamp(replica, value, ts, downstreamReplicas);
                    ts++;
                }
            }
        }

        #endregion

        #region WithVectorClock

        public void Benchmark_Add_WithVectorClock()
        {
            TestType value;
            TCRDT replica;
            List<TCRDT> downstreamReplicas;

            var clock = new VectorClock(_nodes);

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _replicas[_nodes[i]];
                downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < _iterations; j++)
                {
                    // Add
                    value = _addObjects[i * _iterations + j];
                    AddWithVectorClock(replica, value, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);
                }
            }
        }

        public void Benchmark_AddAndUpdate_WithVectorClock()
        {
            TestType value;
            TCRDT replica;
            List<TCRDT> downstreamReplicas;

            var clock = new VectorClock(_nodes);

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _replicas[_nodes[i]];
                downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < _iterations; j++)
                {
                    // Add
                    value = _addObjects[i * _iterations + j];
                    AddWithVectorClock(replica, value, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);

                    // Update
                    value = _updateObjects[i * _iterations + j];
                    UpdateWithVectorClock(replica, value, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);
                }
            }
        }

        public void Benchmark_AddAndRemove_WithVectorClock()
        {
            TestType value;
            TCRDT replica;
            List<TCRDT> downstreamReplicas;

            var clock = new VectorClock(_nodes);

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _replicas[_nodes[i]];
                downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < _iterations; j++)
                {
                    // Add
                    value = _addObjects[i * _iterations + j];
                    AddWithVectorClock(replica, value, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);

                    // Remove
                    RemoveWithVectorClock(replica, value, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);
                }
            }
        }

        public void Benchmark_AddUpdateAndRemove_WithVectorClock()
        {
            TestType value;
            TCRDT replica;
            List<TCRDT> downstreamReplicas;

            var clock = new VectorClock(_nodes);

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _replicas[_nodes[i]];
                downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < _iterations; j++)
                {
                    // Add
                    value = _addObjects[i * _iterations + j];
                    AddWithVectorClock(replica, value, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);

                    // Update
                    value = _updateObjects[i * _iterations + j];
                    UpdateWithVectorClock(replica, value, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);

                    // Remove
                    RemoveWithVectorClock(replica, value, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);
                }
            }
        }

        #endregion

        private List<Guid> GenerateGuids(int count)
        {
            var guids = new List<Guid>(count);

            for (int i = 0; i < count; i++)
            {
                guids.Add(Guid.NewGuid());
            }

            return guids;
        }

        private List<TestType> GenerateObjects(Random random, List<Guid> ids)
        {
            var objects = new List<TestType>(ids.Count);

            objects.AddRange(ids.Select(id => new TestTypeBuilder(random).Build(id)));

            return objects;
        }
    }
}