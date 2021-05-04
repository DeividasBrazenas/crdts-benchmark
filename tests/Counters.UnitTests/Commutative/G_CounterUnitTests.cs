using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Counters.Commutative.GrowOnly;
using CRDT.Counters.Entities;
using Xunit;

namespace CRDT.Counters.UnitTests.Commutative
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

            var sum = counter.Sum();

            Assert.Equal(33, sum);
        }
    }
}
