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
    public class LWW_SetWithVCTests
    {
        [Theory]
        [AutoData]
        public void Create_CreatesSetWithElements(LWW_SetWithVCElement<TestType> one, LWW_SetWithVCElement<TestType> two, LWW_SetWithVCElement<TestType> three)
        {
            var adds = new[] { one, two }.ToImmutableHashSet();
            var removes = new[] { two, three }.ToImmutableHashSet();

            var lwwSet = new LWW_SetWithVC<TestType>(adds, removes);

            Assert.Equal(adds.Count, lwwSet.Adds.Count);
            Assert.Equal(removes.Count, lwwSet.Removes.Count);

            foreach (var add in adds)
            {
                Assert.Contains(add, lwwSet.Adds);
            }

            foreach (var remove in removes)
            {
                Assert.Contains(remove, lwwSet.Removes);
            }
        }

        [Theory]
        [AutoData]
        public void Add_AddsElementToAddsSet(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_SetWithVC<TestType>();

            var add = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)));

            lwwSet = lwwSet.Add(add.Value, add.VectorClock);

            Assert.Contains(add, lwwSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_AddSameElementTwiceWithDifferentTimestamp_UpdatesExistingElement(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_SetWithVC<TestType>();

            var firstAdd = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)));
            var secondAdd = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)));

            lwwSet = lwwSet.Add(firstAdd.Value, firstAdd.VectorClock);
            lwwSet = lwwSet.Add(secondAdd.Value, secondAdd.VectorClock);

            Assert.True(lwwSet.Adds.Count(e => Equals(e, secondAdd)) == 1);
            Assert.True(lwwSet.Adds.Count(e => Equals(e, firstAdd)) == 0);
        }

        [Theory]
        [AutoData]
        public void Add_ConcurrentAdds_AddsOnlyOne(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_SetWithVC<TestType>();

            var firstAdd = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)));
            var secondAdd = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)));

            lwwSet = lwwSet.Add(firstAdd.Value, firstAdd.VectorClock);
            lwwSet = lwwSet.Add(secondAdd.Value, secondAdd.VectorClock);

            Assert.Equal(1, lwwSet.Adds.Count(e => Equals(e.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_SetWithVC<TestType>();

            var newLwwSet = lwwSet.Remove(value, new VectorClock(clock.Add(node, 0)));

            Assert.Same(lwwSet, newLwwSet);
        }

        [Theory]
        [AutoData]
        public void Remove_RemovesElementToRemovesSet(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_SetWithVC<TestType>();

            var add = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)));
            var remove = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)));

            lwwSet = lwwSet.Add(add.Value, add.VectorClock);
            lwwSet = lwwSet.Remove(remove.Value, remove.VectorClock);

            Assert.Contains(remove, lwwSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Remove_RemoveSameElementTwiceWithDifferentTimestamp_AddsOneElement(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_SetWithVC<TestType>();

            var add = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)));
            var firstRemove = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)));
            var secondRemove = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 2)));

            lwwSet = lwwSet.Add(add.Value, add.VectorClock);
            lwwSet = lwwSet.Remove(firstRemove.Value, firstRemove.VectorClock);
            lwwSet = lwwSet.Remove(secondRemove.Value, secondRemove.VectorClock);

            Assert.True(lwwSet.Removes.Count(e => Equals(e.Value, value)) == 1);
        }

        [Theory]
        [AutoData]
        public void Remove_ConcurrentRemoves_AddsOnlyOne(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_SetWithVC<TestType>();

            var add = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)));
            var firstRemove = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)));
            var secondRemove = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)));

            lwwSet = lwwSet.Add(add.Value, add.VectorClock);
            lwwSet = lwwSet.Remove(firstRemove.Value, firstRemove.VectorClock);
            lwwSet = lwwSet.Remove(secondRemove.Value, secondRemove.VectorClock);

            Assert.Equal(1, lwwSet.Removes.Count(e => Equals(e.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_SetWithVC<TestType>();

            lwwSet = lwwSet.Add(value, new VectorClock(clock.Add(node, 0)));

            var lookup = lwwSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_SetWithVC<TestType>();

            var add = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)));
            var remove = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)));

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
            var lwwSet = new LWW_SetWithVC<TestType>();

            var add = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)));
            var remove = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)));
            var reAdd = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 2)));

            lwwSet = lwwSet.Add(add.Value, add.VectorClock);
            lwwSet = lwwSet.Remove(remove.Value, remove.VectorClock);
            lwwSet = lwwSet.Add(reAdd.Value, reAdd.VectorClock);

            var lookup = lwwSet.Lookup(value);

            Assert.True(lookup);
        }
    }
}
