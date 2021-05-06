using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Commutative.Set;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Core.Cluster;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Application.UnitTests.Commutative
{
    public class G_SetServiceTests
    {
        private readonly IG_SetRepository<TestType> _repository;
        private readonly G_SetService<TestType> _gSetService;

        public G_SetServiceTests()
        {
            _repository = new G_SetRepository();
            _gSetService = new G_SetService<TestType>(_repository);
        }

        [Theory]
        [AutoData]
        public void Add_NoExistingValues_AddsElementsToTheRepository(TestType value)
        {
            _gSetService.DownstreamAdd(value);

            var repositoryValues = _repository.GetValues();
            Assert.Equal(1, repositoryValues.Count(v => Equals(v, value)));
        }

        [Theory]
        [AutoData]
        public void Add_WithExistingValues_AddsElementsToTheRepository(ImmutableHashSet<TestType> existingValues, TestType value)
        {
            _repository.PersistValues(existingValues);

            _gSetService.DownstreamAdd(value);

            var repositoryValues = _repository.GetValues();
            Assert.Equal(1, repositoryValues.Count(v => Equals(v, value)));
        }

        [Theory]
        [AutoData]
        public void Add_IsIdempotent(ImmutableHashSet<TestType> existingValues, TestType value)
        {
            _repository.PersistValues(existingValues);

            _gSetService.DownstreamAdd(value);
            _gSetService.DownstreamAdd(value);
            _gSetService.DownstreamAdd(value);

            var repositoryValues = _repository.GetValues();
            Assert.Equal(1, repositoryValues.Count(v => Equals(v, value)));
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsTrue(ImmutableHashSet<TestType> existingValues, TestType value)
        {
            _repository.PersistValues(existingValues);

            _gSetService.DownstreamAdd(value);

            var lookup = _gSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsFalse(ImmutableHashSet<TestType> existingValues, TestType value)
        {
            _repository.PersistValues(existingValues);

            var lookup = _gSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Fact]
        public void Commutative_Add_NewValue()
        {
            var nodes = CreateNodes(3);
            var commutativeReplicas = CreateCommutativeReplicas(nodes);
            var objectsCount = 1000;
            var random = new Random();
            var objects = TestTypeBuilder.Build(Guid.NewGuid(), objectsCount);
            TestType value;

            foreach (var replica in commutativeReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    value = objects[random.Next(objectsCount)];

                    replica.Value.LocalAdd(value);

                    CommutativeDownstreamAdd(replica.Key.Id, value, commutativeReplicas);
                }
            }

            var state = commutativeReplicas.First().Value.State;

            foreach (var replica in commutativeReplicas)
            {
                Assert.Equal(state, replica.Value.State);
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


        private Dictionary<Node, G_SetService<TestType>> CreateCommutativeReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, G_SetService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new G_SetRepository();
                var service = new G_SetService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private bool CommutativeDownstreamAdd(Guid senderId, TestType value, Dictionary<Node, G_SetService<TestType>> replicas)
        {
            var downstreamReplicas = replicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.DownstreamAdd(value);
            }

            return true;
        }

        private void AssertContains(ImmutableHashSet<TestType> expectedValues, IEnumerable<TestType> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}