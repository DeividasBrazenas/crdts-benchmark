using System;
using System.Collections.Immutable;
using AutoFixture.Xunit2;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Convergent.ObservedUpdatedRemoved;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;
using static CRDT.UnitTestHelpers.TestTypes.TestTypeBuilder;

namespace CRDT.Sets.UnitTests.Convergent
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
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_SetWithVC<TestType>();

            ourSet = ourSet.Merge(new[] { new OUR_SetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0))) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);

            var lookup = ourSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_SetWithVC<TestType>();
            ourSet = ourSet.Merge(new[] { new OUR_SetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0))) }.ToImmutableHashSet(), new[] { new OUR_SetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0))) }.ToImmutableHashSet());

            var lookup = ourSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SameValueWithSeveralTags_ReturnsTrue(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_SetWithVC<TestType>();

            ourSet = ourSet.Merge(new[] { new OUR_SetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0))) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);
            ourSet = ourSet.Merge(new[] { new OUR_SetWithVCElement<TestType>(value, Guid.NewGuid(), new VectorClock(clock.Add(node, 1))) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);
            ourSet = ourSet.Merge(ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty, new[] { new OUR_SetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 2))) }.ToImmutableHashSet());

            var lookup = ourSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedRemovedAndUpdated_ReturnsTrue(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_SetWithVC<TestType>();

            ourSet = ourSet.Merge(new[] { new OUR_SetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0))) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);
            ourSet = ourSet.Merge(ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty, new[] { new OUR_SetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 1))) }.ToImmutableHashSet());

            var newValue = Build(value.Id);

            ourSet = ourSet.Merge(new[] { new OUR_SetWithVCElement<TestType>(newValue, tag, new VectorClock(clock.Add(node, 2))) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);

            var lookup = ourSet.Lookup(newValue);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Values_ReturnsNonRemovedValues(TestType one, TestType two, TestType three, Guid tagOne, Guid tagTwo, Guid tagThree, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_SetWithVC<TestType>();

            ourSet = ourSet.Merge(new[] { new OUR_SetWithVCElement<TestType>(one, tagOne, new VectorClock(clock.Add(node, 0))) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);
            ourSet = ourSet.Merge(new[] { new OUR_SetWithVCElement<TestType>(one, tagOne, new VectorClock(clock.Add(node, 0))) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);
            ourSet = ourSet.Merge(ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty, new[] { new OUR_SetWithVCElement<TestType>(one, tagTwo, new VectorClock(clock.Add(node, 0))) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_SetWithVCElement<TestType>(two, tagTwo, new VectorClock(clock.Add(node, 0))) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);
            ourSet = ourSet.Merge(new[] { new OUR_SetWithVCElement<TestType>(two, tagTwo, new VectorClock(clock.Add(node, 0))) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);
            ourSet = ourSet.Merge(new[] { new OUR_SetWithVCElement<TestType>(two, tagOne, new VectorClock(clock.Add(node, 0))) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);
            ourSet = ourSet.Merge(ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty, new[] { new OUR_SetWithVCElement<TestType>(two, tagOne, new VectorClock(clock.Add(node, 0))) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty, new[] { new OUR_SetWithVCElement<TestType>(three, tagThree, new VectorClock(clock.Add(node, 0))) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_SetWithVCElement<TestType>(three, tagThree, new VectorClock(clock.Add(node, 0))) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty);
            ourSet = ourSet.Merge(ImmutableHashSet<OUR_SetWithVCElement<TestType>>.Empty, new[] { new OUR_SetWithVCElement<TestType>(three, tagThree, new VectorClock(clock.Add(node, 0))) }.ToImmutableHashSet());

            var actualValues = ourSet.Values;

            Assert.Equal(2, actualValues.Count);
            Assert.Contains(one, actualValues);
            Assert.Contains(two, actualValues);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesAddsAndRemoves(OUR_SetWithVCElement<TestType> one, OUR_SetWithVCElement<TestType> two,
            OUR_SetWithVCElement<TestType> three, OUR_SetWithVCElement<TestType> four, OUR_SetWithVCElement<TestType> five)
        {
            var ourSet = new OUR_SetWithVC<TestType>(new[] { one, two }.ToImmutableHashSet(), new[] { three }.ToImmutableHashSet());

            var newOrSet = ourSet.Merge(new[] { three, four }.ToImmutableHashSet(), new[] { five }.ToImmutableHashSet());

            Assert.Equal(4, newOrSet.Adds.Count);
            Assert.Equal(1, newOrSet.Removes.Count);
            Assert.Contains(one, newOrSet.Adds);
            Assert.Contains(two, newOrSet.Adds);
            Assert.Contains(three, newOrSet.Adds);
            Assert.Contains(four, newOrSet.Adds);
            Assert.Contains(three, newOrSet.Removes);
        }
    }
}
