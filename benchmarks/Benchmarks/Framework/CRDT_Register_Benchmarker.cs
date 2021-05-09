using System;
using System.Collections.Generic;
using System.Linq;
using Benchmarks.TestTypes;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using Newtonsoft.Json.Linq;

namespace Benchmarks.Framework
{
    public class CRDT_Register_Benchmarker<TCRDT>
    {
        private int _iterations;
        private readonly List<Node> _nodes;
        private readonly Dictionary<Node, TCRDT> _replicas;
        private readonly List<TestType> _addObjects;
        private readonly List<TestType> _updateObjects;


        public Action<TCRDT, TestType, long, List<TCRDT>> ConvergentAssignWithTimestamp { get; set; }
        public Action<TCRDT, TestType, long, List<TCRDT>> ConvergentRemoveWithTimestamp { get; set; }

        public Action<TCRDT, TestType, VectorClock, List<TCRDT>> ConvergentAssignWithVectorClock { get; set; }
        public Action<TCRDT, TestType, VectorClock, List<TCRDT>> ConvergentRemoveWithVectorClock { get; set; }

        public Action<TCRDT, Guid, JToken, long, List<TCRDT>> CommutativeAssignWithTimestamp { get; set; }
        public Action<TCRDT, TestType, long, List<TCRDT>> CommutativeRemoveWithTimestamp { get; set; }

        public Action<TCRDT, JToken, VectorClock, List<TCRDT>> CommutativeAssignWithVectorClock { get; set; }
        public Action<TCRDT, JToken, VectorClock, List<TCRDT>> CommutativeRemoveWithVectorClock { get; set; }

        public CRDT_Register_Benchmarker(int iterations, List<Node> nodes, Dictionary<Node, TCRDT> replicas)
        {
            _iterations = iterations;
            _nodes = nodes;
            _replicas = replicas;
            var objectIds = GenerateGuids(iterations * _nodes.Count);
            var random = new Random();
            _addObjects = GenerateObjects(random, objectIds);
            _updateObjects = GenerateObjects(random, objectIds);
        }

        #region WithTimestamp

        public void Benchmark_Convergent_Assign_WithTimestamp()
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
                    // Assign
                    value = _addObjects[i * _iterations + j];
                    ConvergentAssignWithTimestamp(replica, value, ts, downstreamReplicas);
                    ts++;
                }
            }
        }

        public void Benchmark_Commutative_Assign_WithTimestamp()
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
                    // Assign
                    value = _addObjects[i * _iterations + j];
                    CommutativeAssignWithTimestamp(replica, value.Id, JToken.FromObject(value), ts, downstreamReplicas);
                    ts++;
                }
            }
        }

        public void Benchmark_Convergent_AssignSingleProperty_WithTimestamp()
        {
            TestType value = _addObjects[0];
            TCRDT replica;
            List<TCRDT> downstreamReplicas;

            var ts = 1;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _replicas[_nodes[i]];
                downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < _iterations; j++)
                {
                    // Assign
                    value.StringValue = Guid.NewGuid().ToString();
                    ConvergentAssignWithTimestamp(replica, value, ts, downstreamReplicas);
                    ts++;
                }
            }
        }

        public void Benchmark_Commutative_AssignSingleProperty_WithTimestamp()
        {
            TestType value = _addObjects[0];
            TCRDT replica;
            List<TCRDT> downstreamReplicas;

            var ts = 1;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _replicas[_nodes[i]];
                downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < _iterations; j++)
                {
                    // Assign
                    value.StringValue = Guid.NewGuid().ToString();
                    var jToken = JToken.Parse($"{{\"StringValue\":\"{value.StringValue}\"}}");
                    CommutativeAssignWithTimestamp(replica, value.Id, jToken, ts, downstreamReplicas);
                    ts++;
                }
            }
        }

        public void Benchmark_Convergent_AssignAndRemove_WithTimestamp()
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
                    // Assign
                    value = _addObjects[i * _iterations + j];
                    ConvergentAssignWithTimestamp(replica, value, ts, downstreamReplicas);
                    ts++;

                    // Remove
                    ConvergentRemoveWithTimestamp(replica, value, ts, downstreamReplicas);
                    ts++;
                }
            }
        }

        public void Benchmark_Commutative_AssignAndRemove_WithTimestamp()
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
                    // Assign
                    value = _addObjects[i * _iterations + j];
                    CommutativeAssignWithTimestamp(replica, value.Id, JToken.FromObject(value),  ts, downstreamReplicas);
                    ts++;

                    // Remove
                    CommutativeRemoveWithTimestamp(replica, value, ts, downstreamReplicas);
                    ts++;
                }
            }
        }

        #endregion

        #region WithVectorClock

        //public void Benchmark_Add_WithVectorClock()
        //{
        //    TestType value;
        //    TCRDT replica;
        //    List<TCRDT> downstreamReplicas;

        //    var clock = new VectorClock(_nodes);

        //    for (int i = 0; i < _nodes.Count; i++)
        //    {
        //        replica = _replicas[_nodes[i]];
        //        downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

        //        for (int j = 0; j < _iterations; j++)
        //        {
        //            // Add
        //            value = _addObjects[i * _iterations + j];
        //            ConvergentAddWithVectorClock(replica, value, clock, downstreamReplicas);
        //            clock = clock.Increment(_nodes[i]);
        //        }
        //    }
        //}

        //public void Benchmark_AddAndUpdate_WithVectorClock()
        //{
        //    TestType value;
        //    TCRDT replica;
        //    List<TCRDT> downstreamReplicas;

        //    var clock = new VectorClock(_nodes);

        //    for (int i = 0; i < _nodes.Count; i++)
        //    {
        //        replica = _replicas[_nodes[i]];
        //        downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

        //        for (int j = 0; j < _iterations; j++)
        //        {
        //            // Add
        //            value = _addObjects[i * _iterations + j];
        //            ConvergentAddWithVectorClock(replica, value, clock, downstreamReplicas);
        //            clock = clock.Increment(_nodes[i]);

        //            // Update
        //            value = _updateObjects[i * _iterations + j];
        //            ConvergentUpdateWithVectorClock(replica, value, clock, downstreamReplicas);
        //            clock = clock.Increment(_nodes[i]);
        //        }
        //    }
        //}

        //public void Benchmark_AddAndRemove_WithVectorClock()
        //{
        //    TestType value;
        //    TCRDT replica;
        //    List<TCRDT> downstreamReplicas;

        //    var clock = new VectorClock(_nodes);

        //    for (int i = 0; i < _nodes.Count; i++)
        //    {
        //        replica = _replicas[_nodes[i]];
        //        downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

        //        for (int j = 0; j < _iterations; j++)
        //        {
        //            // Add
        //            value = _addObjects[i * _iterations + j];
        //            ConvergentAddWithVectorClock(replica, value, clock, downstreamReplicas);
        //            clock = clock.Increment(_nodes[i]);

        //            // Remove
        //            ConvergentRemoveWithVectorClock(replica, value, clock, downstreamReplicas);
        //            clock = clock.Increment(_nodes[i]);
        //        }
        //    }
        //}

        //public void Benchmark_AddUpdateAndRemove_WithVectorClock()
        //{
        //    TestType value;
        //    TCRDT replica;
        //    List<TCRDT> downstreamReplicas;

        //    var clock = new VectorClock(_nodes);

        //    for (int i = 0; i < _nodes.Count; i++)
        //    {
        //        replica = _replicas[_nodes[i]];
        //        downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

        //        for (int j = 0; j < _iterations; j++)
        //        {
        //            // Add
        //            value = _addObjects[i * _iterations + j];
        //            ConvergentAddWithVectorClock(replica, value, clock, downstreamReplicas);
        //            clock = clock.Increment(_nodes[i]);

        //            // Update
        //            value = _updateObjects[i * _iterations + j];
        //            ConvergentUpdateWithVectorClock(replica, value, clock, downstreamReplicas);
        //            clock = clock.Increment(_nodes[i]);

        //            // Remove
        //            ConvergentRemoveWithVectorClock(replica, value, clock, downstreamReplicas);
        //            clock = clock.Increment(_nodes[i]);
        //        }
        //    }
        //}

        #endregion

        private List<Guid> GenerateGuids(int count)
        {
            var guid = Guid.NewGuid();
            var guids = new List<Guid>(count);

            for (int i = 0; i < count; i++)
            {
                guids.Add(guid);
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