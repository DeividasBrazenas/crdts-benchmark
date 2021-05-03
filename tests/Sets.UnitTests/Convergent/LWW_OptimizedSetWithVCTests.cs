using System;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Convergent;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Sets.UnitTests.Convergent
{
    public class LWW_OptimizedSetWithVCTests
    {
        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var lwwSet = new LWW_OptimizedSetWithVC<TestType>();
            lwwSet = lwwSet.Merge(new[] { new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false) }.ToImmutableHashSet());

            var lookup = lwwSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var lwwSet = new LWW_OptimizedSetWithVC<TestType>();

            lwwSet = lwwSet.Merge(new[] { new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false) }.ToImmutableHashSet());
            lwwSet = lwwSet.Merge(new[] { new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)), true) }.ToImmutableHashSet());

            var lookup = lwwSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsTrue(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var lwwSet = new LWW_OptimizedSetWithVC<TestType>();

            var add = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false);
            var remove = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)), true);
            var reAdd = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 2)), false);

            lwwSet = lwwSet.Merge(new[] { new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false) }.ToImmutableHashSet());
            lwwSet = lwwSet.Merge(new[] { new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)), true) }.ToImmutableHashSet());
            lwwSet = lwwSet.Merge(new[] { new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 2)), false) }.ToImmutableHashSet());

            var lookup = lwwSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesAddsAndRemoves(TestType one, TestType two, TestType three, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var elementOne = new LWW_OptimizedSetWithVCElement<TestType>(one, new VectorClock(clock.Add(node, 0)), false);
            var elementTwo = new LWW_OptimizedSetWithVCElement<TestType>(two, new VectorClock(clock.Add(node, 1)), true);
            var elementThree = new LWW_OptimizedSetWithVCElement<TestType>(one, new VectorClock(clock.Add(node, 2)), true);
            var elementFour = new LWW_OptimizedSetWithVCElement<TestType>(three, new VectorClock(clock.Add(node, 3)), false);
            var elementFive = new LWW_OptimizedSetWithVCElement<TestType>(two, new VectorClock(clock.Add(node, 0)), true);

            var lwwSet = new LWW_OptimizedSetWithVC<TestType>(new[] { elementOne, elementTwo }.ToImmutableHashSet());

            var newLwwSet = lwwSet.Merge(new[] { elementThree, elementFour, elementFive }.ToImmutableHashSet());

            Assert.Equal(3, newLwwSet.Elements.Count);
            Assert.Contains(newLwwSet.Elements, e => Equals(e, elementTwo));
            Assert.Contains(newLwwSet.Elements, e => Equals(e, elementThree));
            Assert.Contains(newLwwSet.Elements, e => Equals(e, elementFour));
        }
    }
}
