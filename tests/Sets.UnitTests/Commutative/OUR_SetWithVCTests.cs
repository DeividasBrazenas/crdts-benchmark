using System;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Commutative.ObservedUpdatedRemoved;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;
using static CRDT.UnitTestHelpers.TestTypes.TestTypeBuilder;

namespace CRDT.Sets.UnitTests.Commutative
{
    public class OUR_SetWithVCTests
    {
        [Theory]
        [AutoData]
        public void Create_CreatesSetWithElements(OUR_SetWithVCElement<TestType> one, OUR_SetWithVCElement<TestType> two,
           OUR_SetWithVCElement<TestType> three)
        {
            var adds = new[] { one, two }.ToImmutableHashSet();
            var removes = new[] { two, three }.ToImmutableHashSet();

            var ourSet = new OUR_SetWithVC<TestType>(adds, removes);

            Assert.Equal(adds.Count, ourSet.Adds.Count);
            Assert.Equal(removes.Count, ourSet.Removes.Count);

            foreach (var add in adds)
            {
                Assert.Contains(add, ourSet.Adds);
            }

            foreach (var remove in removes)
            {
                Assert.Contains(remove, ourSet.Removes);
            }
        }

        [Theory]
        [AutoData]
        public void Add_AddsElementToAddsSet(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_SetWithVC<TestType>();

            ourSet = ourSet.Add(value, tag, new VectorClock(clock.Add(node, 0)));

            var element = new OUR_SetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)));
            Assert.Contains(element, ourSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_Concurrent_AddsOnlyOneElement(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_SetWithVC<TestType>();

            ourSet = ourSet.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Add(value, tag, new VectorClock(clock.Add(node, 0)));

            var element = new OUR_SetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)));
            Assert.Equal(1, ourSet.Adds.Count(v => Equals(v, element)));
        }

        [Theory]
        [AutoData]
        public void Update_UpdatesElementInAddsSet(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_SetWithVC<TestType>();

            var newValue = Build(value.Id);
            var newElement = new OUR_SetWithVCElement<TestType>(newValue, tag, new VectorClock(clock.Add(node, 1)));

            ourSet = ourSet.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Update(newValue, tag, new VectorClock(clock.Add(node, 1)));

            var element = new OUR_SetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)));

            Assert.Contains(newElement, ourSet.Adds);
            Assert.DoesNotContain(element, ourSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Update_NotExistingValue_DoesNotAddToTheAddsSet(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_SetWithVC<TestType>();

            ourSet = ourSet.Update(value, tag, new VectorClock(clock.Add(node, 0)));

            var element = new OUR_SetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)));

            Assert.DoesNotContain(element, ourSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_SetWithVC<TestType>();

            var newOrSet = ourSet.Remove(value, tag, new VectorClock(clock.Add(node, 0)));

            Assert.Same(ourSet, newOrSet);
        }

        [Theory]
        [AutoData]
        public void Remove_AddsElementToRemovesSet(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_SetWithVC<TestType>();

            ourSet = ourSet.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Remove(value, tag, new VectorClock(clock.Add(node, 0)));

            var element = new OUR_SetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)));

            Assert.Contains(element, ourSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Remove_Concurrent_AddsOnlyOneElementToRemoveSet(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_SetWithVC<TestType>();

            ourSet = ourSet.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Remove(value, tag, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Remove(value, tag, new VectorClock(clock.Add(node, 0)));

            var element = new OUR_SetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)));

            Assert.Equal(1, ourSet.Removes.Count(v => Equals(v, element)));
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_SetWithVC<TestType>();

            ourSet = ourSet.Add(value, tag, new VectorClock(clock.Add(node, 0)));

            var lookup = ourSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_SetWithVC<TestType>();

            ourSet = ourSet.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Remove(value, tag, new VectorClock(clock.Add(node, 0)));

            var lookup = ourSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SameValueWithSeveralTags_ReturnsTrue(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_SetWithVC<TestType>();

            ourSet = ourSet.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Add(value, Guid.NewGuid(), new VectorClock(clock.Add(node, 1)));
            ourSet = ourSet.Remove(value, tag, new VectorClock(clock.Add(node, 2)));

            var lookup = ourSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedRemovedAndUpdated_ReturnsTrue(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_SetWithVC<TestType>();

            ourSet = ourSet.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Remove(value, tag, new VectorClock(clock.Add(node, 1)));

            var newValue = Build(value.Id);

            ourSet = ourSet.Update(newValue, tag, new VectorClock(clock.Add(node, 1)));

            var lookup = ourSet.Lookup(newValue);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Values_ReturnsNonRemovedValues(TestType one, TestType two, TestType three, Guid tagOne, Guid tagTwo, Guid tagThree, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_SetWithVC<TestType>();

            ourSet = ourSet.Add(one, tagOne, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Add(one, tagTwo, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Remove(one, tagTwo, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Add(two, tagTwo, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Add(two, tagOne, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Remove(two, tagOne, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Remove(three, tagThree, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Add(three, tagThree, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Remove(three, tagThree, new VectorClock(clock.Add(node, 0)));

            var actualValues = ourSet.Values;

            Assert.Equal(2, actualValues.Count);
            Assert.Contains(one, actualValues);
            Assert.Contains(two, actualValues);
        }
    }
}
