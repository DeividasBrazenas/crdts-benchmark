using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Counters.Convergent;
using CRDT.Counters.Convergent.GrowOnly;
using CRDT.Counters.Entities;
using Xunit;

namespace CRDT.Counters.UnitTests.Convergent
{
    public class G_CounterUnitTests
    {
        [Theory]
        [AutoData]
        public void Add_AddsNewNodeToElements(int add, Guid nodeId)
        {
            var counter = new G_Counter(ImmutableHashSet<CounterElement>.Empty);

            counter = counter.Add(add, nodeId);

            var element = counter.Elements.FirstOrDefault(e => e.Node.Id == nodeId);

            Assert.Equal(add, element.Value);
        }

        [Theory]
        [AutoData]
        public void Add_AddsToExistingElement(HashSet<CounterElement> existingElements, int add, Guid nodeId)
        {
            var existingElement = new CounterElement(999, nodeId);
            existingElements.Add(existingElement);

            var counter = new G_Counter(existingElements.ToImmutableHashSet());

            counter = counter.Add(add, nodeId);

            var element = counter.Elements.FirstOrDefault(e => e.Node.Id == nodeId);

            Assert.Equal(999 + add, element.Value);
        }

        [Theory]
        [AutoData]
        public void Sum_TakesSumOfAllElements(Guid nodeOneId, Guid nodeTwoId, Guid nodeThreeId)
        {
            var elements = new List<CounterElement> { new(7, nodeOneId), new(17, nodeTwoId), new(9, nodeThreeId) };

            var counter = new G_Counter(elements.ToImmutableHashSet());

            var sum = counter.Sum;

            Assert.Equal(33, sum);
        }

        [Theory]
        [AutoData]
        public void Merge_TakesMaxOfElements(Guid nodeOneId, Guid nodeTwoId, Guid nodeThreeId, Guid nodeFourId)
        {
            var elements = new List<CounterElement> { new(7, nodeOneId), new(17, nodeTwoId), new(9, nodeThreeId) };
            var otherElements = new List<CounterElement> { new(3, nodeTwoId), new(42, nodeThreeId), new(10, nodeFourId) };

            var counter = new G_Counter(elements.ToImmutableHashSet());

            counter = counter.Merge(otherElements.ToImmutableHashSet());

            Assert.Equal(4, counter.Elements.Count);
            Assert.Equal(1, counter.Elements.Count(e => e.Value == 7 && e.Node.Id == nodeOneId));
            Assert.Equal(1, counter.Elements.Count(e => e.Value == 17 && e.Node.Id == nodeTwoId));
            Assert.Equal(1, counter.Elements.Count(e => e.Value == 42 && e.Node.Id == nodeThreeId));
            Assert.Equal(1, counter.Elements.Count(e => e.Value == 10 && e.Node.Id == nodeFourId));
        }

        [Theory]
        [AutoData]
        public void Merge_IsCommutative(Guid nodeOneId, Guid nodeTwoId, Guid nodeThreeId, Guid nodeFourId)
        {
            var elements = new List<CounterElement> { new(7, nodeOneId), new(17, nodeTwoId), new(9, nodeThreeId) };
            var otherElements = new List<CounterElement> { new(3, nodeTwoId), new(42, nodeThreeId), new(10, nodeFourId) };

            var counter = new G_Counter(elements.ToImmutableHashSet());

            counter = counter.Merge(otherElements.ToImmutableHashSet());
            counter = counter.Merge(otherElements.ToImmutableHashSet());
            counter = counter.Merge(otherElements.ToImmutableHashSet());
            counter = counter.Merge(otherElements.ToImmutableHashSet());

            Assert.Equal(4, counter.Elements.Count);
            Assert.Equal(1, counter.Elements.Count(e => e.Value == 7 && e.Node.Id == nodeOneId));
            Assert.Equal(1, counter.Elements.Count(e => e.Value == 17 && e.Node.Id == nodeTwoId));
            Assert.Equal(1, counter.Elements.Count(e => e.Value == 42 && e.Node.Id == nodeThreeId));
            Assert.Equal(1, counter.Elements.Count(e => e.Value == 10 && e.Node.Id == nodeFourId));
        }
    }
}
