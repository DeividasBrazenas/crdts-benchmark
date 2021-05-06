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
    public class P_SetServiceTests
    {
        private readonly IP_SetRepository<TestType> _repository;
        private readonly P_SetService<TestType> _pSetService;
        private readonly TestTypeBuilder _builder;

        public P_SetServiceTests()
        {
            _repository = new P_SetRepository();
            _pSetService = new P_SetService<TestType>(_repository);
            _builder = new TestTypeBuilder(new Random());
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value)
        {
            _pSetService.Merge(new HashSet<TestType> { value }.ToImmutableHashSet(), ImmutableHashSet<TestType>.Empty);

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(HashSet<TestType> values)
        {
            _pSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<TestType>.Empty);

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SingleElement_AddsElementsToTheRepository(HashSet<TestType> existingValues, TestType value)
        {
            _repository.PersistAdds(existingValues.ToImmutableHashSet());

            _pSetService.Merge(new HashSet<TestType> { value }.ToImmutableHashSet(), ImmutableHashSet<TestType>.Empty);

            var repositoryValues = _repository.GetAdds();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeAdds_SeveralElements_AddsElementsToTheRepository(HashSet<TestType> existingValues, HashSet<TestType> values)
        {
            _repository.PersistAdds(existingValues.ToImmutableHashSet());

            _pSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<TestType>.Empty);

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeAdds_IsIdempotent(HashSet<TestType> values, TestType value)
        {
            _repository.PersistAdds(values.ToImmutableHashSet());

            values.Add(value);

            _pSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<TestType>.Empty);
            _pSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<TestType>.Empty);
            _pSetService.Merge(values.ToImmutableHashSet(), ImmutableHashSet<TestType>.Empty);

            var repositoryValues = _repository.GetAdds();
            AssertContains(values, repositoryValues);
        }

        [Fact]
        public void MergeAdds_IsCommutative()
        {
            var firstValue = _builder.Build();
            var secondValue = _builder.Build();
            var thirdValue = _builder.Build();
            var fourthValue = _builder.Build();
            var fifthValue = _builder.Build();

            var firstRepository = new P_SetRepository();
            var firstService = new P_SetService<TestType>(firstRepository);

            _repository.PersistAdds(new HashSet<TestType> { firstValue, secondValue, thirdValue }.ToImmutableHashSet());
            firstService.Merge(new HashSet<TestType> { fourthValue, fifthValue }.ToImmutableHashSet(), ImmutableHashSet<TestType>.Empty);

            var firstRepositoryValues = firstRepository.GetAdds();

            var secondRepository = new P_SetRepository();
            var secondService = new P_SetService<TestType>(secondRepository);

            _repository.PersistAdds(new HashSet<TestType> { fourthValue, fifthValue }.ToImmutableHashSet());
            secondService.Merge(new HashSet<TestType> { firstValue, secondValue, thirdValue }.ToImmutableHashSet(), ImmutableHashSet<TestType>.Empty);

            var secondRepositoryValues = firstRepository.GetAdds();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleValueWithEmptyRepository_AddsElementsToTheRepository(TestType value)
        {
            _repository.PersistAdds(new HashSet<TestType> { value }.ToImmutableHashSet());
            _pSetService.Merge(ImmutableHashSet<TestType>.Empty, new HashSet<TestType> { value }.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SeveralElementsWithEmptyRepository_AddsElementsToTheRepository(HashSet<TestType> values)
        {
            _repository.PersistAdds(values.ToImmutableHashSet());
            _pSetService.Merge(ImmutableHashSet<TestType>.Empty, values.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SingleElement_AddsElementsToTheRepository(HashSet<TestType> existingValues, TestType value)
        {
            _repository.PersistRemoves(existingValues.ToImmutableHashSet());
            _repository.PersistAdds(new HashSet<TestType> { value }.ToImmutableHashSet());

            _pSetService.Merge(ImmutableHashSet<TestType>.Empty, new HashSet<TestType> { value }.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            Assert.Equal(1, repositoryValues.Count(x => Equals(x, value)));
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_SeveralElements_AddsElementsToTheRepository(HashSet<TestType> existingValues, HashSet<TestType> values)
        {
            _repository.PersistRemoves(existingValues.ToImmutableHashSet());
            _repository.PersistAdds(values.ToImmutableHashSet());

            _pSetService.Merge(ImmutableHashSet<TestType>.Empty, values.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Theory]
        [AutoData]
        public void MergeRemoves_IsIdempotent(HashSet<TestType> values, TestType value)
        {
            _repository.PersistRemoves(values.ToImmutableHashSet());
            _repository.PersistAdds(new HashSet<TestType> { value }.ToImmutableHashSet());

            values.Add(value);

            _pSetService.Merge(ImmutableHashSet<TestType>.Empty, values.ToImmutableHashSet());
            _pSetService.Merge(ImmutableHashSet<TestType>.Empty, values.ToImmutableHashSet());
            _pSetService.Merge(ImmutableHashSet<TestType>.Empty, values.ToImmutableHashSet());

            var repositoryValues = _repository.GetRemoves();
            AssertContains(values, repositoryValues);
        }

        [Fact]
        public void MergeRemoves_IsCommutative()
        {
            var firstValue = _builder.Build();
            var secondValue = _builder.Build();
            var thirdValue = _builder.Build();
            var fourthValue = _builder.Build();
            var fifthValue = _builder.Build();

            var firstRepository = new P_SetRepository();
            var firstService = new P_SetService<TestType>(firstRepository);

            firstRepository.PersistRemoves(new HashSet<TestType> { firstValue, secondValue, thirdValue }.ToImmutableHashSet());
            firstRepository.PersistAdds(new HashSet<TestType> { fourthValue, fifthValue }.ToImmutableHashSet());
            firstService.Merge(ImmutableHashSet<TestType>.Empty, new HashSet<TestType> { fourthValue, fifthValue }.ToImmutableHashSet());

            var firstRepositoryValues = firstRepository.GetRemoves();

            var secondRepository = new P_SetRepository();
            var secondService = new P_SetService<TestType>(secondRepository);

            secondRepository.PersistRemoves(new HashSet<TestType> { fourthValue, fifthValue }.ToImmutableHashSet());
            secondRepository.PersistAdds(new HashSet<TestType> { firstValue, secondValue, thirdValue }.ToImmutableHashSet());
            secondService.Merge(ImmutableHashSet<TestType>.Empty, new HashSet<TestType> { firstValue, secondValue, thirdValue }.ToImmutableHashSet());

            var secondRepositoryValues = firstRepository.GetRemoves();

            Assert.Equal(firstRepositoryValues, secondRepositoryValues);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsTrue(TestType value)
        {
            _pSetService.Merge(new HashSet<TestType> { value }.ToImmutableHashSet(), ImmutableHashSet<TestType>.Empty);

            var lookup = _pSetService.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReturnsFalse(TestType value)
        {
            _pSetService.Merge(new HashSet<TestType> { value }.ToImmutableHashSet(), new HashSet<TestType> { value }.ToImmutableHashSet());

            var lookup = _pSetService.Lookup(value);

            Assert.False(lookup);
        }

        [Fact]
        public void Convergent_AddNewValue()
        {
            var nodes = CreateNodes(3);
            var convergentReplicas = CreateConvergentReplicas(nodes);
            var random = new Random();
            var objectsCount = 1000;
            var objects = new TestTypeBuilder(random).Build(Guid.NewGuid(), objectsCount);

            TestType value;

            foreach (var replica in convergentReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    value = objects[random.Next(objectsCount)];

                    replica.Value.LocalAdd(value);

                    var (adds, removes) = replica.Value.State;

                    ConvergentDownstreamMerge(replica.Key.Id, adds, removes, convergentReplicas);
                }
            }

            var (expectedAdds, expectedRemoves) = convergentReplicas.First().Value.State;

            foreach (var replica in convergentReplicas)
            {
                var (actualAdds, actualRemoves) = replica.Value.State;

                Assert.Equal(expectedAdds, actualAdds);
                Assert.Equal(expectedRemoves, actualRemoves);
            }
        }

        [Fact]
        public void Convergent_AddAndRemoveValue()
        {
            var nodes = CreateNodes(3);
            var convergentReplicas = CreateConvergentReplicas(nodes);
            var random = new Random();
            var objectsCount = 1000;
            var objects = new TestTypeBuilder(random).Build(Guid.NewGuid(), objectsCount);

            TestType value;

            foreach (var replica in convergentReplicas)
            {
                for (int i = 0; i < 100; i++)
                {
                    value = objects[random.Next(objectsCount)];

                    replica.Value.LocalAdd(value);

                    var (adds, removes) = replica.Value.State;

                    ConvergentDownstreamMerge(replica.Key.Id, adds, removes, convergentReplicas);


                    replica.Value.LocalRemove(value);

                    (adds, removes) = replica.Value.State;

                    ConvergentDownstreamMerge(replica.Key.Id, adds, removes, convergentReplicas);
                }
            }

            var (expectedAdds, expectedRemoves) = convergentReplicas.First().Value.State;

            foreach (var replica in convergentReplicas)
            {
                var (actualAdds, actualRemoves) = replica.Value.State;

                Assert.Equal(expectedAdds, actualAdds);
                Assert.Equal(expectedRemoves, actualRemoves);
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

        private Dictionary<Node, P_SetService<TestType>> CreateConvergentReplicas(List<Node> nodes)
        {
            var dictionary = new Dictionary<Node, P_SetService<TestType>>();

            foreach (var node in nodes)
            {
                var repository = new P_SetRepository();
                var service = new P_SetService<TestType>(repository);

                dictionary.Add(node, service);
            }

            return dictionary;
        }

        private void ConvergentDownstreamMerge(Guid senderId, ImmutableHashSet<TestType> adds, ImmutableHashSet<TestType> removes, Dictionary<Node, P_SetService<TestType>> replicas)
        {
            var downstreamReplicas = replicas.Where(r => r.Key.Id != senderId);

            foreach (var downstreamReplica in downstreamReplicas)
            {
                downstreamReplica.Value.Merge(adds, removes);
            }
        }

        private void AssertContains(HashSet<TestType> expectedValues, IEnumerable<TestType> actualValues)
        {
            foreach (var value in expectedValues)
            {
                Assert.Equal(1, actualValues.Count(v => Equals(v, value)));
            }
        }
    }
}