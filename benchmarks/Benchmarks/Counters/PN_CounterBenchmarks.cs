using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using CRDT.Benchmarks.Repositories;
using CRDT.Core.Cluster;
using CRDT.Counters.Entities;

namespace Benchmarks.Counters
{
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    public class PN_CounterBenchmarks
    {
        private List<Node> _nodes;
        private Dictionary<Node, CRDT.Application.Commutative.Counter.PN_CounterService> _commutativeReplicas;
        private Dictionary<Node, CRDT.Application.Convergent.Counter.PN_CounterService> _convergentReplicas;

        [GlobalSetup]
        public void Setup()
        {
            _nodes = CreateNodes(3);
            _commutativeReplicas = CreateCommutativeReplicas(_nodes);
            _convergentReplicas = CreateConvergentReplicas(_nodes);
        }

        [Benchmark]
        public void Commutative_Add_NetworkOK()
        {
            for (int i = 1; i <= 100; i++)
            {
                Parallel.ForEach(_commutativeReplicas, replica =>
                {
                    replica.Value.Add(i, replica.Key.Id);

                    CommutativeDownstreamAdd(replica.Key.Id, i);
                });
            }
        }

        [Benchmark]
        public void Convergent_Add_NetworkOK()
        {
            for (int i = 1; i <= 100; i++)
            {
                Parallel.ForEach(_convergentReplicas, replica =>
                {
                    replica.Value.LocalAdd(i, replica.Key.Id);

                    var (additions, subtractions) = replica.Value.State;

                    ConvergentDownstreamMerge(replica.Key.Id, additions, subtractions);
                });
            }
        }

        [Benchmark]
        public void Convergent_Add_Every2ndNodeDidNotReceiveUpdateImmediately()
        {
            for (int i = 1; i <= 100; i++)
            {
                Parallel.ForEach(_convergentReplicas, replica =>
                {
                    replica.Value.LocalAdd(i, replica.Key.Id);

                    var (additions, subtractions) = replica.Value.State;

                    ConvergentMergeDownstreamWithNetworkFailures(replica.Key.Id, additions, subtractions);
                });
            }
        }

        [Benchmark]
        public void Commutative_Subtract_NetworkOK()
        {
            for (int i = 1; i <= 100; i++)
            {
                Parallel.ForEach(_commutativeReplicas, replica =>
                {
                    replica.Value.Subtract(i, replica.Key.Id);

                    CommutativeDownstreamSubtract(replica.Key.Id, i);
                });
            }
        }

        [Benchmark]
        public void Convergent_Subtract_NetworkOK()
        {
            for (int i = 1; i <= 100; i++)
            {
                Parallel.ForEach(_convergentReplicas, replica =>
                {
                    replica.Value.LocalSubtract(i, replica.Key.Id);

                    var (additions, subtractions) = replica.Value.State;

                    ConvergentDownstreamMerge(replica.Key.Id, additions, subtractions);
                });
            }
        }

        [Benchmark]
        public void Convergent_Subtract_Every2ndNodeDidNotReceiveUpdateImmediately()
        {
            for (int i = 1; i <= 100; i++)
            {
                Parallel.ForEach(_convergentReplicas, replica =>
                {
                    replica.Value.LocalSubtract(i, replica.Key.Id);

                    var (additions, subtractions) = replica.Value.State;

                    ConvergentMergeDownstreamWithNetworkFailures(replica.Key.Id, additions, subtractions);
                });
            }
        }

        [Benchmark]
        public void Commutative_AddAndSubtract_NetworkOK()
        {
            for (int i = 1; i <= 100; i++)
            {
                if (i % 2 == 0)
                {
                    Parallel.ForEach(_commutativeReplicas, replica =>
                    {
                        replica.Value.Add(i, replica.Key.Id);

                        CommutativeDownstreamAdd(replica.Key.Id, i);
                    });
                }
                else
                {
                    Parallel.ForEach(_commutativeReplicas, replica =>
                    {
                        replica.Value.Subtract(i, replica.Key.Id);

                        CommutativeDownstreamSubtract(replica.Key.Id, i);
                    });
                }
            }
        }

        [Benchmark]
        public void Convergent_AddAndSubtract_NetworkOK()
        {
            for (int i = 1; i <= 100; i++)
            {
                if (i % 2 == 0)
                {
                    Parallel.ForEach(_convergentReplicas, replica =>
                    {
                        replica.Value.LocalAdd(i, replica.Key.Id);

                        var (additions, subtractions) = replica.Value.State;

                        ConvergentDownstreamMerge(replica.Key.Id, additions, subtractions);
                    });
                }
                else
                {
                    Parallel.ForEach(_convergentReplicas, replica =>
                    {
                        replica.Value.LocalSubtract(i, replica.Key.Id);

                        var (additions, subtractions) = replica.Value.State;

                        ConvergentDownstreamMerge(replica.Key.Id, additions, subtractions);
                    });
                }
            }
        }

        [Benchmark]
        public void Convergent_AddAndSubtract_Every2ndNodeDidNotReceiveUpdateImmediately()
        {
            for (int i = 1; i <= 100; i++)
            {
                if (i % 2 == 0)
                {
                    Parallel.ForEach(_convergentReplicas, replica =>
                    {
                        replica.Value.LocalAdd(i, replica.Key.Id);

                        var (additions, subtractions) = replica.Value.State;

                        ConvergentMergeDownstreamWithNetworkFailures(replica.Key.Id, additions, subtractions);
                    });
                }
                else
                {
                    Parallel.ForEach(_convergentReplicas, replica =>
                    {
                        replica.Value.LocalSubtract(i, replica.Key.Id);

                        var (additions, subtractions) = replica.Value.State;

                        ConvergentMergeDownstreamWithNetworkFailures(replica.Key.Id, additions, subtractions);
                    });
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

        private Dictionary<Node, CRDT.Application.Commutative.Counter.PN_CounterService> CreateCommutativeReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Commutative.Counter.PN_CounterService>();

            foreach (var node in nodes)
            {
                var repository = new PN_CounterRepository();
                var service = new CRDT.Application.Commutative.Counter.PN_CounterService(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void CommutativeDownstreamAdd(Guid senderId, int value)
        {
            var downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.Add(value, senderId);
            }
        }

        private void CommutativeDownstreamSubtract(Guid senderId, int value)
        {
            var downstreamReplicas = _commutativeReplicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.Subtract(value, senderId);
            }
        }
        #endregion

        #region Convergent

        private Dictionary<Node, CRDT.Application.Convergent.Counter.PN_CounterService> CreateConvergentReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, CRDT.Application.Convergent.Counter.PN_CounterService>();

            foreach (var node in nodes)
            {
                var repository = new PN_CounterRepository();
                var service = new CRDT.Application.Convergent.Counter.PN_CounterService(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void ConvergentDownstreamMerge(Guid senderId, IEnumerable<CounterElement> additions, IEnumerable<CounterElement> subtractions)
        {
            var downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.Merge(additions, subtractions);
            }
        }

        private void ConvergentMergeDownstreamWithNetworkFailures(Guid senderId, IEnumerable<CounterElement> additions, IEnumerable<CounterElement> subtractions)
        {
            var downstreamReplicas = _convergentReplicas.Where(r => r.Key.Id != senderId).Where((x, i) => i % 2 == 0);
            var replicasWithoutUpdate = _convergentReplicas.Except(downstreamReplicas).Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.Merge(additions, subtractions);

                foreach (var replicaWithoutUpdate in replicasWithoutUpdate)
                {
                    var (downstreamAdditions, downstreamSubtractions) = downstreamReplica.Value.State;

                    replicaWithoutUpdate.Value.Merge(downstreamAdditions, downstreamSubtractions);
                }
            }
        }

        #endregion
    }
}