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
        private readonly List<TestType> _objects;

        public Action<TCRDT, TestType, long, List<TCRDT>> Add { get; set; }
        public Action<TCRDT, TestType, long, List<TCRDT>> Update { get; set; }
        public Action<TCRDT, TestType, long, List<TCRDT>> Remove { get; set; }

        public Action<TCRDT, TestType, long, List<TCRDT>> AddWithTimestamp { get; set; }
        public Action<TCRDT, TestType, long, List<TCRDT>> UpdateWithTimestamp { get; set; }
        public Action<TCRDT, TestType, long, List<TCRDT>> RemoveWithTimestamp { get; set; }

        public Action<TCRDT, TestType, VectorClock, List<TCRDT>> AddWithVectorClock { get; set; }
        public Action<TCRDT, TestType, VectorClock, List<TCRDT>> UpdateWithVectorClock { get; set; }
        public Action<TCRDT, TestType, VectorClock, List<TCRDT>> RemoveWithVectorClock { get; set; }

        public CRDT_Set_Benchmarker(int iterations, List<Node> nodes, Dictionary<Node, TCRDT> replicas, List<TestType> objects)
        {
            _iterations = iterations;
            _nodes = nodes;
            _replicas = replicas;
            _objects = objects;
        }

        #region WithTimestamp

        public void Benchmark_Add_WithTimestamp()
        {
            TestType value;
            TCRDT replica;
            List<TCRDT> downstreamReplicas;

            var ts = 0;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _replicas[_nodes[i]];
                downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < _iterations; j++)
                {
                    // Add
                    value = _objects[i * _iterations + j];
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

            var ts = 0;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _replicas[_nodes[i]];
                downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < _iterations; j++)
                {
                    // Add
                    value = _objects[i * _iterations + j];
                    AddWithTimestamp(replica, value, ts, downstreamReplicas);
                    ts++;

                    // Update
                    value = _objects[2 * i * _iterations + j];
                    UpdateWithTimestamp(replica, value, ts, downstreamReplicas);
                    ts++;
                }
            }
        }

        public void Benchmark_AddAndRemove_WithTimestamp()
        {
            TestType value;
            TestType newValue;
            TCRDT replica;
            List<TCRDT> downstreamReplicas;

            var ts = 0;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _replicas[_nodes[i]];
                downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < _iterations; j++)
                {
                    // Add
                    value = _objects[i * _iterations + j];
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

            var ts = 0;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _replicas[_nodes[i]];
                downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < _iterations; j++)
                {
                    // Add
                    value = _objects[i * _iterations + j];
                    AddWithTimestamp(replica, value, ts, downstreamReplicas);
                    ts++;

                    // Update
                    value = _objects[2 * i * _iterations + j];
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
                    value = _objects[i * _iterations + j];
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
                    value = _objects[i * _iterations + j];
                    AddWithVectorClock(replica, value, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);

                    // Update
                    value = _objects[2 * i * _iterations + j];
                    UpdateWithVectorClock(replica, value, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);
                }
            }
        }

        public void Benchmark_AddAndRemove_WithVectorClock()
        {
            TestType value;
            TestType newValue;
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
                    value = _objects[i * _iterations + j];
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
                    value = _objects[i * _iterations + j];
                    AddWithVectorClock(replica, value, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);

                    // Update
                    value = _objects[2 * i * _iterations + j];
                    UpdateWithVectorClock(replica, value, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);

                    // Remove
                    RemoveWithVectorClock(replica, value, clock, downstreamReplicas);
                    clock = clock.Increment(_nodes[i]);
                }
            }
        }

        #endregion

    }
}