using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Application.Convergent.Set;
using CRDT.Application.Interfaces;
using CRDT.Application.UnitTests.Repositories;
using CRDT.Core.Cluster;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;
using static CRDT.UnitTestHelpers.TestTypes.TestTypeBuilder;

namespace CRDT.Application.UnitTests.Convergent
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
        public void Merge_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value)
        {
            _gSetService.Merge(new List<TestType> { value });

            var repositoryValues = _repository.GetValues();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void Merge_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(List<TestType> values)
        {
            _gSetService.Merge(values);

            var repositoryValues = _repository.GetValues();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Merge_SingleElement_AddsElementsToTheRepository(List<TestType> existingValues, TestType value)
        {
            _repository.PersistValues(existingValues);

            _gSetService.Merge(new List<TestType> { value });

            var repositoryValues = _repository.GetValues();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void Merge_SeveralElements_AddsElementsToTheRepository(List<TestType> existingValues, List<TestType> values)
        {
            _repository.PersistValues(existingValues);

            _gSetService.Merge(values);

            var repositoryValues = _repository.GetValues();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void Merge_IsIdempotent(List<TestType> values, TestType value)
        {
            _repository.PersistValues(values);

            values.Add(value);

            _gSetService.Merge(values);
            _gSetService.Merge(values);
            _gSetService.Merge(values);

            var repositoryValues = _repository.GetValues();
            AssertContains(values, repositoryValues);
        }

        [Fact]
        public void Merge_IsCommutative()
        {
            var firstValue = Build();
            var secondValue = Build();
            var thirdValue = Build();
            var fourthValue = Build();
            var fifthValue = Build();

            var firstRepository = new G_SetRepository();
            var firstService = new G_SetService<TestType>(firstRepository);

            firstRepository.PersistValues(new List<TestType> { firstValue, secondValue, thirdValue });
            firstService.Merge(new List<TestType> { fourthValue, fifthValue });

            var firstRepositoryValues = firstRepository.GetValues();

            var secondRepository = new G_SetRepository();
            var secondService = new G_SetService<TestType>(secondRepository);

            secondRepository.PersistValues(new List<TestType> { fourthValue, fifthValue });
            secondService.Merge(new List<TestType> { firstValue, secondValue, thirdValue });

            var secondRepositoryValues = firstRepository.GetValues();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsTrue(List<TestType> values, TestType value)
        {
            _repository.PersistValues(values);

            _gSetService.Merge(new List<TestType> { value });

            var lookup = _gSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsFalse(List<TestType> values, TestType value)
        {
            _repository.PersistValues(values);

            var lookup = _gSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Fact]
        public void Convergent_Add_NewValue()
        {
            var nodes = CreateNodes(3);
            var convergentReplicas = CreateConvergentReplicas(nodes);
            var random = new Random();
            var objectsCount = 1000;
            var objects = Build(Guid.NewGuid(), objectsCount);
            TestType value;
            var expectedObjects = new HashSet<TestType>();

            foreach (var replica in convergentReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    value = objects[random.Next(objectsCount)];

                    replica.Value.LocalAdd(value);

                    ConvergentDownstreamMerge(replica.Key.Id, replica.Value.State, convergentReplicas);

                    expectedObjects.Add(value);
                }
            }

            var state = convergentReplicas.First().Value.State;

            foreach (var replica in convergentReplicas)
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

        private Dictionary<Node, G_SetService<TestType>> CreateConvergentReplicas(List<Node> nodes)
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

        private bool ConvergentDownstreamMerge(Guid senderId, IEnumerable<TestType> state, Dictionary<Node, G_SetService<TestType>> replicas)
        {
            var downstreamReplicas = replicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.Merge(state);
            }

            return true;
        }

        private void AssertContains(List<TestType> expectedValues, IEnumerable<TestType> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}