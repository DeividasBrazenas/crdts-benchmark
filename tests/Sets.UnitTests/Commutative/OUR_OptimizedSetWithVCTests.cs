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

namespace CRDT.Sets.UnitTests.Commutative
{
    public class OUR_OptimizedSetWithVCTests
    {
        private readonly TestTypeBuilder _builder;

        public OUR_OptimizedSetWithVCTests()
        {
            _builder = new TestTypeBuilder(new Random());
        }

        [Theory]
        [AutoData]
        public void Create_CreatesSetWithElements(OUR_OptimizedSetWithVCElement<TestType> one, OUR_OptimizedSetWithVCElement<TestType> two,
           OUR_OptimizedSetWithVCElement<TestType> three)
        {
            var elements = new[] { one, two, three }.ToImmutableHashSet();
            
            var ourSet = new OUR_OptimizedSetWithVC<TestType>(elements);

            Assert.Equal(elements.Count, ourSet.Elements.Count);

            foreach (var element in elements)
            {
                Assert.Contains(element, ourSet.Elements);
            }
        }

        [Theory]
        [AutoData]
        public void Add_AddsElementToAddsSet(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_OptimizedSetWithVC<TestType>();

            ourSet = ourSet.Add(value, tag, new VectorClock(clock.Add(node, 0)));

            var element = new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)), false);
            Assert.Contains(element, ourSet.Elements);
        }

        [Theory]
        [AutoData]
        public void Add_Concurrent_AddsOnlyOneElement(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_OptimizedSetWithVC<TestType>();

            ourSet = ourSet.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Add(value, tag, new VectorClock(clock.Add(node, 0)));

            var element = new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)), false);
            Assert.Equal(1, ourSet.Elements.Count(v => Equals(v, element)));
        }

        [Theory]
        [AutoData]
        public void Update_UpdatesElementInAddsSet(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_OptimizedSetWithVC<TestType>();

            var newValue = _builder.Build(value.Id);
            var newElement = new OUR_OptimizedSetWithVCElement<TestType>(newValue, tag, new VectorClock(clock.Add(node, 1)), false);

            ourSet = ourSet.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Update(newValue, tag, new VectorClock(clock.Add(node, 1)));

            var element = new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)), false);

            Assert.Contains(newElement, ourSet.Elements);
            Assert.DoesNotContain(element, ourSet.Elements);
        }

        [Theory]
        [AutoData]
        public void Update_NotExistingValue_DoesNotAddToTheAddsSet(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_OptimizedSetWithVC<TestType>();

            ourSet = ourSet.Update(value, tag, new VectorClock(clock.Add(node, 0)));

            var element = new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)), false);

            Assert.DoesNotContain(element, ourSet.Elements);
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_OptimizedSetWithVC<TestType>();

            var newOrSet = ourSet.Remove(value, tag, new VectorClock(clock.Add(node, 0)));

            Assert.Same(ourSet, newOrSet);
        }

        [Theory]
        [AutoData]
        public void Remove_AddsElementToRemovesSet(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_OptimizedSetWithVC<TestType>();

            ourSet = ourSet.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Remove(value, tag, new VectorClock(clock.Add(node, 0)));

            var element = new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)), true);

            Assert.Contains(element, ourSet.Elements);
        }

        [Theory]
        [AutoData]
        public void Remove_Concurrent_AddsOnlyOneElementToRemoveSet(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_OptimizedSetWithVC<TestType>();

            ourSet = ourSet.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Remove(value, tag, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Remove(value, tag, new VectorClock(clock.Add(node, 0)));

            var element = new OUR_OptimizedSetWithVCElement<TestType>(value, tag, new VectorClock(clock.Add(node, 0)), true);

            Assert.Equal(1, ourSet.Elements.Count(v => Equals(v, element)));
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_OptimizedSetWithVC<TestType>();

            ourSet = ourSet.Add(value, tag, new VectorClock(clock.Add(node, 0)));

            var lookup = ourSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_OptimizedSetWithVC<TestType>();

            ourSet = ourSet.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Remove(value, tag, new VectorClock(clock.Add(node, 1)));

            var lookup = ourSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SameValueWithSeveralTags_ReturnsTrue(TestType value, Guid tag, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_OptimizedSetWithVC<TestType>();

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

            var ourSet = new OUR_OptimizedSetWithVC<TestType>();

            ourSet = ourSet.Add(value, tag, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Remove(value, tag, new VectorClock(clock.Add(node, 1)));

            var newValue = _builder.Build(value.Id);

            ourSet = ourSet.Update(newValue, tag, new VectorClock(clock.Add(node, 1)));

            var lookup = ourSet.Lookup(newValue);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Values_ReturnsNonRemovedValues(TestType one, TestType two, TestType three, Guid tagOne, Guid tagTwo, Guid tagThree, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var ourSet = new OUR_OptimizedSetWithVC<TestType>();

            ourSet = ourSet.Add(one, tagOne, new VectorClock(clock.Add(node, 0)));
            ourSet = ourSet.Add(one, tagTwo, new VectorClock(clock.Add(node, 1)));
            ourSet = ourSet.Remove(one, tagTwo, new VectorClock(clock.Add(node,2)));
            ourSet = ourSet.Add(two, tagTwo, new VectorClock(clock.Add(node, 3)));
            ourSet = ourSet.Add(two, tagOne, new VectorClock(clock.Add(node, 4)));
            ourSet = ourSet.Remove(two, tagOne, new VectorClock(clock.Add(node, 5)));
            ourSet = ourSet.Remove(three, tagThree, new VectorClock(clock.Add(node, 6)));
            ourSet = ourSet.Add(three, tagThree, new VectorClock(clock.Add(node, 7)));
            ourSet = ourSet.Remove(three, tagThree, new VectorClock(clock.Add(node, 8)));

            var actualValues = ourSet.Values;

            Assert.Equal(2, actualValues.Count);
            Assert.Contains(one, actualValues);
            Assert.Contains(two, actualValues);
        }
    }
}
