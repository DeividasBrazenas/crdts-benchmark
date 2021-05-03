using System;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Commutative;
using CRDT.Sets.Commutative.LastWriterWins;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Sets.UnitTests.Commutative
{
    public class LWW_OptimizedSetWithVCTests
    {
        [Theory]
        [AutoData]
        public void Add_AddsElementToAddsSet(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_OptimizedSetWithVC<TestType>();

            var add = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false);

            lwwSet = lwwSet.Add(add.Value, new VectorClock(clock.Add(node, 0)));

            Assert.Contains(add, lwwSet.Elements);
        }

        [Theory]
        [AutoData]
        public void Add_AddSameElementTwiceWithDifferentTimestamp_UpdatesTimestamp(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_OptimizedSetWithVC<TestType>();

            var firstAdd = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false);
            var secondAdd = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)), false);

            lwwSet = lwwSet.Add(firstAdd.Value, firstAdd.VectorClock);
            lwwSet = lwwSet.Add(secondAdd.Value, secondAdd.VectorClock);

            Assert.True(lwwSet.Elements.Count(e => Equals(e, firstAdd)) == 0);
            Assert.True(lwwSet.Elements.Count(e => Equals(e, secondAdd)) == 1);
        }

        [Theory]
        [AutoData]
        public void Add_ConcurrentAdds_AddsOnlyOne(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_OptimizedSetWithVC<TestType>();

            var firstAdd = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false);
            var secondAdd = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false);

            lwwSet = lwwSet.Add(firstAdd.Value, firstAdd.VectorClock);
            lwwSet = lwwSet.Add(secondAdd.Value, secondAdd.VectorClock);

            Assert.Equal(1, lwwSet.Elements.Count(e => Equals(e.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(LWW_OptimizedSetWithVCElement<TestType> element)
        {
            var lwwSet = new LWW_OptimizedSetWithVC<TestType>();

            var newLwwSet = lwwSet.Remove(element.Value, element.VectorClock);

            Assert.Same(lwwSet, newLwwSet);
        }

        [Theory]
        [AutoData]
        public void Remove_RemovesElementToRemovesSet(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_OptimizedSetWithVC<TestType>();

            var add = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false);
            var remove = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)), true);

            lwwSet = lwwSet.Add(add.Value, add.VectorClock);
            lwwSet = lwwSet.Remove(remove.Value, remove.VectorClock);

            Assert.DoesNotContain(add, lwwSet.Elements);
            Assert.Contains(remove, lwwSet.Elements);
        }

        [Theory]
        [AutoData]
        public void Remove_RemoveSameElementTwiceWithDifferentTimestamp_AddsOneElements(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_OptimizedSetWithVC<TestType>();

            var add = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false);
            var firstRemove = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)), true);
            var secondRemove = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 2)), true);

            lwwSet = lwwSet.Add(add.Value, add.VectorClock);
            lwwSet = lwwSet.Remove(firstRemove.Value, firstRemove.VectorClock);
            lwwSet = lwwSet.Remove(secondRemove.Value, secondRemove.VectorClock);

            Assert.True(lwwSet.Elements.Count(e => Equals(e.Value, value)) == 1);
        }

        [Theory]
        [AutoData]
        public void Remove_ConcurrentRemoves_AddsOnlyOne(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_OptimizedSetWithVC<TestType>();

            var add = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false);
            var firstRemove = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)), true);
            var secondRemove = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)), true);

            lwwSet = lwwSet.Add(add.Value, add.VectorClock);
            lwwSet = lwwSet.Remove(firstRemove.Value, firstRemove.VectorClock);
            lwwSet = lwwSet.Remove(secondRemove.Value, secondRemove.VectorClock);

            Assert.Equal(1, lwwSet.Elements.Count(e => Equals(e.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(LWW_OptimizedSetWithVCElement<TestType> element)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_OptimizedSetWithVC<TestType>();

            lwwSet = lwwSet.Add(element.Value, element.VectorClock);

            var lookup = lwwSet.Lookup(element.Value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_OptimizedSetWithVC<TestType>();

            var add = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)), false);
            var remove = new LWW_OptimizedSetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)), true);

            lwwSet = lwwSet.Add(add.Value, add.VectorClock);
            lwwSet = lwwSet.Remove(remove.Value, remove.VectorClock);

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

            lwwSet = lwwSet.Add(add.Value, add.VectorClock);
            lwwSet = lwwSet.Remove(remove.Value, remove.VectorClock);
            lwwSet = lwwSet.Add(reAdd.Value, reAdd.VectorClock);

            var lookup = lwwSet.Lookup(value);

            Assert.True(lookup);
        }
    }
}
