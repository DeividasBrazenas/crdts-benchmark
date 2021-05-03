using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Counters.Convergent;
using CRDT.Counters.Convergent.PositiveNegative;
using CRDT.Counters.Entities;
using Xunit;

namespace CRDT.Counters.UnitTests.Convergent
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

        [Theory]
        [AutoData]
        public void Merge_TakesMaxOfElements(Guid nodeOneId, Guid nodeTwoId, Guid nodeThreeId, Guid nodeFourId)
        {
            var additions = new List<CounterElement> { new(7, nodeOneId), new(17, nodeTwoId), new(9, nodeThreeId) };
            var otherAdditions = new List<CounterElement> { new(3, nodeTwoId), new(42, nodeThreeId), new(10, nodeFourId) };

            var subtractions = new List<CounterElement> { new(2, nodeOneId), new(9, nodeTwoId), new(5, nodeThreeId) };
            var otherSubtractions = new List<CounterElement> { new(3, nodeTwoId), new(11, nodeThreeId), new(7, nodeFourId) };

            var counter = new PN_Counter(additions.ToImmutableHashSet(), subtractions.ToImmutableHashSet());

            counter = counter.Merge(otherAdditions.ToImmutableHashSet(), otherSubtractions.ToImmutableHashSet());

            Assert.Equal(4, counter.Additions.Count);
            Assert.Equal(1, counter.Additions.Count(e => e.Value == 7 && e.Node.Id == nodeOneId));
            Assert.Equal(1, counter.Additions.Count(e => e.Value == 17 && e.Node.Id == nodeTwoId));
            Assert.Equal(1, counter.Additions.Count(e => e.Value == 42 && e.Node.Id == nodeThreeId));
            Assert.Equal(1, counter.Additions.Count(e => e.Value == 10 && e.Node.Id == nodeFourId));

            Assert.Equal(4, counter.Subtractions.Count);
            Assert.Equal(1, counter.Subtractions.Count(e => e.Value == 2 && e.Node.Id == nodeOneId));
            Assert.Equal(1, counter.Subtractions.Count(e => e.Value == 9 && e.Node.Id == nodeTwoId));
            Assert.Equal(1, counter.Subtractions.Count(e => e.Value == 11 && e.Node.Id == nodeThreeId));
            Assert.Equal(1, counter.Subtractions.Count(e => e.Value == 7 && e.Node.Id == nodeFourId));
        }

        [Theory]
        [AutoData]
        public void Merge_IsCommutative(Guid nodeOneId, Guid nodeTwoId, Guid nodeThreeId, Guid nodeFourId)
        {
            var additions = new List<CounterElement> { new(7, nodeOneId), new(17, nodeTwoId), new(9, nodeThreeId) };
            var otherAdditions = new List<CounterElement> { new(3, nodeTwoId), new(42, nodeThreeId), new(10, nodeFourId) };

            var subtractions = new List<CounterElement> { new(2, nodeOneId), new(9, nodeTwoId), new(5, nodeThreeId) };
            var otherSubtractions = new List<CounterElement> { new(3, nodeTwoId), new(11, nodeThreeId), new(7, nodeFourId) };

            var counter = new PN_Counter(additions.ToImmutableHashSet(), subtractions.ToImmutableHashSet());

            counter = counter.Merge(otherAdditions.ToImmutableHashSet(), otherSubtractions.ToImmutableHashSet());
            counter = counter.Merge(otherAdditions.ToImmutableHashSet(), otherSubtractions.ToImmutableHashSet());
            counter = counter.Merge(otherAdditions.ToImmutableHashSet(), otherSubtractions.ToImmutableHashSet());
            counter = counter.Merge(otherAdditions.ToImmutableHashSet(), otherSubtractions.ToImmutableHashSet());

            Assert.Equal(4, counter.Additions.Count);
            Assert.Equal(1, counter.Additions.Count(e => e.Value == 7 && e.Node.Id == nodeOneId));
            Assert.Equal(1, counter.Additions.Count(e => e.Value == 17 && e.Node.Id == nodeTwoId));
            Assert.Equal(1, counter.Additions.Count(e => e.Value == 42 && e.Node.Id == nodeThreeId));
            Assert.Equal(1, counter.Additions.Count(e => e.Value == 10 && e.Node.Id == nodeFourId));

            Assert.Equal(4, counter.Subtractions.Count);
            Assert.Equal(1, counter.Subtractions.Count(e => e.Value == 2 && e.Node.Id == nodeOneId));
            Assert.Equal(1, counter.Subtractions.Count(e => e.Value == 9 && e.Node.Id == nodeTwoId));
            Assert.Equal(1, counter.Subtractions.Count(e => e.Value == 11 && e.Node.Id == nodeThreeId));
            Assert.Equal(1, counter.Subtractions.Count(e => e.Value == 7 && e.Node.Id == nodeFourId));
        }
    }
}
