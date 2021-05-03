using System;
using System.Collections.Immutable;
using AutoFixture.Xunit2;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Convergent;
using CRDT.Sets.Convergent.ObservedUpdatedRemoved;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;
using static CRDT.UnitTestHelpers.TestTypes.TestTypeBuilder;

namespace CRDT.Sets.UnitTests.Convergent
{
    public class OUR_OptimizedSetWithVCTests
    {
        [Theory]
        [AutoData]
        public void Create_CreatesSetWithElements(OUR_OptimizedSetWithVCElement<TestType> one, OUR_OptimizedSetWithVCElement<TestType> two,
                 OUR_OptimizedSetWithVCElement<TestType> three)
        {
            var elements = new[] { one, two, three }.ToImmutableHashSet();

            var ourSet = new OUR_OptimizedSetWithVC<TestType>(elements);

            Assert.Equal(elements.Count, ourSet.Elements.Count);

            foreach (var add in elements)
            {
                Assert.Contains(add, ourSet.Elements);
            }
        }


        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_OptimizedSetWithVC<TestType>();

            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)), false) }.ToImmutableHashSet());

            var lookup = ourSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_OptimizedSetWithVC<TestType>();

            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)), false) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 1)), true) }.ToImmutableHashSet());

            var lookup = ourSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SameValueWithSeveralTags_ReturnsTrue(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_OptimizedSetWithVC<TestType>();

            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)), false) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetWithVCElement<TestType>(value, Guid.NewGuid(), new VectorClock(clock.Add(node, 0)), false) }.ToImmutableHashSet());

            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 2)), true) }.ToImmutableHashSet());

            var lookup = ourSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedRemovedAndUpdated_ReturnsTrue(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_OptimizedSetWithVC<TestType>();

            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)), false) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 1)), true) }.ToImmutableHashSet());
          
            var newValue = Build(value.Id);

            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetWithVCElement<TestType>(newValue, tag, new VectorClock(clock.Add(node, 3)), false) }.ToImmutableHashSet());

            var lookup = ourSet.Lookup(newValue);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Values_ReturnsNonRemovedValues(TestType one, TestType two, TestType three, Guid tagOne, Guid tagTwo, Guid tagThree, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_OptimizedSetWithVC<TestType>();

            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetWithVCElement<TestType>(one, tagOne, new VectorClock(clock.Add(node, 0)), false) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetWithVCElement<TestType>(one, tagOne, new VectorClock(clock.Add(node, 1)), false) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetWithVCElement<TestType>(one, tagTwo, new VectorClock(clock.Add(node, 2)), true) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetWithVCElement<TestType>(two, tagTwo, new VectorClock(clock.Add(node, 3)), false) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetWithVCElement<TestType>(two, tagTwo, new VectorClock(clock.Add(node, 4)), false) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetWithVCElement<TestType>(two, tagOne, new VectorClock(clock.Add(node, 5)), false) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetWithVCElement<TestType>(two, tagOne, new VectorClock(clock.Add(node, 6)), true) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetWithVCElement<TestType>(three, tagThree, new VectorClock(clock.Add(node, 7)), true) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetWithVCElement<TestType>(three, tagThree, new VectorClock(clock.Add(node, 8)), false) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetWithVCElement<TestType>(three, tagThree, new VectorClock(clock.Add(node, 9)), true) }.ToImmutableHashSet());

            var actualValues = ourSet.Values;

            Assert.Equal(2, actualValues.Count);
            Assert.Contains(one, actualValues);
            Assert.Contains(two, actualValues);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesAddsAndRemoves(TestType one, TestType two, TestType three, Guid tagOne, Guid tagTwo, Guid tagThree, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var orSet = new OUR_OptimizedSetWithVC<TestType>(new[] {
                new OUR_OptimizedSetWithVCElement<TestType>(one, tagOne, new VectorClock(clock.Add(node, 1)), false),
                new OUR_OptimizedSetWithVCElement<TestType>(two, tagOne, new VectorClock(clock.Add(node, 7)), true),
                new OUR_OptimizedSetWithVCElement<TestType>(three, tagThree, new VectorClock(clock.Add(node, 3)), false),
            }.ToImmutableHashSet());

            var newOrSet = orSet.Merge(new[] {
                new OUR_OptimizedSetWithVCElement<TestType>(one, tagOne, new VectorClock(clock.Add(node, 4)), false),
                new OUR_OptimizedSetWithVCElement<TestType>(one, tagTwo, new VectorClock(clock.Add(node, 5)), true),
                new OUR_OptimizedSetWithVCElement<TestType>(two, tagOne, new VectorClock(clock.Add(node, 6)), false),
                new OUR_OptimizedSetWithVCElement<TestType>(three, tagThree, new VectorClock(clock.Add(node, 7)), false),
            }.ToImmutableHashSet());

            Assert.Equal(4, newOrSet.Elements.Count);
            Assert.Contains(new OUR_OptimizedSetWithVCElement<TestType>(one, tagOne, new VectorClock(clock.Add(node, 4)), false), newOrSet.Elements);
            Assert.Contains(new OUR_OptimizedSetWithVCElement<TestType>(two, tagOne, new VectorClock(clock.Add(node, 7)), true), newOrSet.Elements);
            Assert.Contains(new OUR_OptimizedSetWithVCElement<TestType>(three, tagThree, new VectorClock(clock.Add(node, 7)), false), newOrSet.Elements);
            Assert.Contains(new OUR_OptimizedSetWithVCElement<TestType>(one, tagTwo, new VectorClock(clock.Add(node, 5)), true), newOrSet.Elements);
        }
    }
}
