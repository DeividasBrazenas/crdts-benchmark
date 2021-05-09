using System;
using System.Collections.Generic;
using System.Linq;
using Benchmarks.TestTypes;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;

namespace Benchmarks.Framework
{
    public class CRDT_Counter_Benchmarker<TCRDT>
    {
        private int _iterations;
        private readonly List<Node> _nodes;
        private readonly Dictionary<Node, TCRDT> _replicas;
        private readonly List<int> _numbers;

        public Action<TCRDT, Guid, int, List<TCRDT>> Add { get; set; }
        public Action<TCRDT, Guid, int, List<TCRDT>> Subtract { get; set; }

        public CRDT_Counter_Benchmarker(int iterations, List<Node> nodes, Dictionary<Node, TCRDT> replicas)
        {
            _iterations = iterations;
            _nodes = nodes;
            _replicas = replicas;
            var random = new Random();
            _numbers = GenerateNumbers(random, nodes.Count * iterations);
        }

        #region WithoutTime

        public void Benchmark_Add()
        {
            TCRDT replica;
            List<TCRDT> downstreamReplicas;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _replicas[_nodes[i]];
                downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < _iterations; j++)
                {
                    Add(replica, _nodes[i].Id, _numbers[i * _iterations + j], downstreamReplicas);
                }
            }
        }

        public void Benchmark_Subtract()
        {
            TCRDT replica;
            List<TCRDT> downstreamReplicas;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _replicas[_nodes[i]];
                downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < _iterations; j++)
                {
                    Subtract(replica, _nodes[i].Id, _numbers[i * _iterations + j], downstreamReplicas);
                }
            }
        }

        public void Benchmark_AddAndSubtract()
        {
            TCRDT replica;
            List<TCRDT> downstreamReplicas;

            for (int i = 0; i < _nodes.Count; i++)
            {
                replica = _replicas[_nodes[i]];
                downstreamReplicas = _replicas.Where(r => r.Key.Id != _nodes[i].Id).Select(v => v.Value).ToList();

                for (int j = 0; j < _iterations; j++)
                {
                    Add(replica, _nodes[i].Id, _numbers[i * _iterations + j], downstreamReplicas);
                    Subtract(replica, _nodes[i].Id, _numbers[i * _iterations + j], downstreamReplicas);
                }
            }
        }

        #endregion

        private List<int> GenerateNumbers(Random random, int count)
        {
            var ints = new List<int>(count);

            for (int i = 0; i < count; i++)
            {
                ints.Add(random.Next());
            }

            return ints;
        }
    }
}