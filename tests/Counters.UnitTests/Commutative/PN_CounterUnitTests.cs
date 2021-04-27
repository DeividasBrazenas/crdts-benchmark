using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Counters.Commutative;
using CRDT.Counters.Entities;
using Xunit;

namespace CRDT.Counters.UnitTests.Commutative
{
    public class PN_CounterUnitTests
    {
        [Theory]
        [AutoData]
        public void Add_AddsNewNodeToElements(int add, Guid nodeId)
        {
            var counter = new PN_Counter(ImmutableHashSet<CounterElement>.Empty, ImmutableHashSet<CounterElement>.Empty);

            counter = counter.Add(add, nodeId);

            var element = counter.Additions.FirstOrDefault(e => e.Node.Id == nodeId);

            Assert.Equal(add, element.Value);
        }

        [Theory]
        [AutoData]
        public void Add_AddsToExistingElement(HashSet<CounterElement> existingElements, int add, Guid nodeId)
        {
            var existingElement = new CounterElement(999, nodeId);
            existingElements.Add(existingElement);

            var counter = new PN_Counter(existingElements.ToImmutableHashSet(), ImmutableHashSet<CounterElement>.Empty);

            counter = counter.Add(add, nodeId);

            var element = counter.Additions.FirstOrDefault(e => e.Node.Id == nodeId);

            Assert.Equal(999 + add, element.Value);
        }

        [Theory]
        [AutoData]
        public void Subtract_AddsNewNodeToElements(int add, Guid nodeId)
        {
            var counter = new PN_Counter(ImmutableHashSet<CounterElement>.Empty, ImmutableHashSet<CounterElement>.Empty);

            counter = counter.Subtract(add, nodeId);

            var element = counter.Subtractions.FirstOrDefault(e => e.Node.Id == nodeId);

            Assert.Equal(add, element.Value);
        }

        [Theory]
        [AutoData]
        public void Subtract_AddsToExistingElement(HashSet<CounterElement> existingElements, int add, Guid nodeId)
        {
            var existingElement = new CounterElement(999, nodeId);
            existingElements.Add(existingElement);

            var counter = new PN_Counter(ImmutableHashSet<CounterElement>.Empty, existingElements.ToImmutableHashSet());

            counter = counter.Subtract(add, nodeId);

            var element = counter.Subtractions.FirstOrDefault(e => e.Node.Id == nodeId);

            Assert.Equal(999 + add, element.Value);
        }

        [Theory]
        [AutoData]
        public void Sum_TakesSumOfAllElements(Guid nodeOneId, Guid nodeTwoId, Guid nodeThreeId)
        {
            var additions = new List<CounterElement> { new(7, nodeOneId), new(17, nodeTwoId), new(9, nodeThreeId) };
            var subtractions = new List<CounterElement> { new(2, nodeOneId), new(3, nodeTwoId), new(1, nodeThreeId) };

            var counter = new PN_Counter(additions.ToImmutableHashSet(), subtractions.ToImmutableHashSet());

            var sum = counter.Sum;

            Assert.Equal(27, sum);
        }
    }
}
