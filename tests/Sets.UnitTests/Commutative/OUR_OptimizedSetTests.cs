using System;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Sets.Commutative.ObservedUpdatedRemoved;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;
using static CRDT.UnitTestHelpers.TestTypes.TestTypeBuilder;

namespace CRDT.Sets.UnitTests.Commutative
{
    public class OUR_OptimizedSetTests
    {
        [Theory]
        [AutoData]
        public void Create_CreatesSetWithElements(OUR_OptimizedSetElement<TestType> one, OUR_OptimizedSetElement<TestType> two,
           OUR_OptimizedSetElement<TestType> three)
        {
            var elements = new[] { one, two, three }.ToImmutableHashSet();
            
            var ourSet = new OUR_OptimizedSet<TestType>(elements);

            Assert.Equal(elements.Count, ourSet.Elements.Count);

            foreach (var element in elements)
            {
                Assert.Contains(element, ourSet.Elements);
            }
        }

        [Theory]
        [AutoData]
        public void Add_AddsElementToAddsSet(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_OptimizedSet<TestType>();

            ourSet = ourSet.Add(value, tag, timestamp);

            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, false);
            Assert.Contains(element, ourSet.Elements);
        }

        [Theory]
        [AutoData]
        public void Add_Concurrent_AddsOnlyOneElement(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_OptimizedSet<TestType>();

            ourSet = ourSet.Add(value, tag, timestamp);
            ourSet = ourSet.Add(value, tag, timestamp);

            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, false);
            Assert.Equal(1, ourSet.Elements.Count(v => Equals(v, element)));
        }

        [Theory]
        [AutoData]
        public void Update_UpdatesElementInAddsSet(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_OptimizedSet<TestType>();

            var newValue = Build(value.Id);
            var newElement = new OUR_OptimizedSetElement<TestType>(newValue, tag, timestamp + 1, false);

            ourSet = ourSet.Add(value, tag, timestamp);
            ourSet = ourSet.Update(newValue, tag, timestamp + 1);

            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, false);

            Assert.Contains(newElement, ourSet.Elements);
            Assert.DoesNotContain(element, ourSet.Elements);
        }

        [Theory]
        [AutoData]
        public void Update_NotExistingValue_DoesNotAddToTheAddsSet(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_OptimizedSet<TestType>();

            ourSet = ourSet.Update(value, tag, timestamp);

            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, false);

            Assert.DoesNotContain(element, ourSet.Elements);
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_OptimizedSet<TestType>();

            var newOrSet = ourSet.Remove(value, tag, timestamp);

            Assert.Same(ourSet, newOrSet);
        }

        [Theory]
        [AutoData]
        public void Remove_AddsElementToRemovesSet(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_OptimizedSet<TestType>();

            ourSet = ourSet.Add(value, tag, timestamp);
            ourSet = ourSet.Remove(value, tag, timestamp);

            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, true);

            Assert.Contains(element, ourSet.Elements);
        }

        [Theory]
        [AutoData]
        public void Remove_Concurrent_AddsOnlyOneElementToRemoveSet(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_OptimizedSet<TestType>();

            ourSet = ourSet.Add(value, tag, timestamp);
            ourSet = ourSet.Remove(value, tag, timestamp);
            ourSet = ourSet.Remove(value, tag, timestamp);

            var element = new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, true);

            Assert.Equal(1, ourSet.Elements.Count(v => Equals(v, element)));
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_OptimizedSet<TestType>();

            ourSet = ourSet.Add(value, tag, timestamp);

            var lookup = ourSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_OptimizedSet<TestType>();

            ourSet = ourSet.Add(value, tag, timestamp);
            ourSet = ourSet.Remove(value, tag, timestamp + 1);

            var lookup = ourSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SameValueWithSeveralTags_ReturnsTrue(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_OptimizedSet<TestType>();

            ourSet = ourSet.Add(value, tag, timestamp);
            ourSet = ourSet.Add(value, Guid.NewGuid(), timestamp + 1);
            ourSet = ourSet.Remove(value, tag, timestamp + 2);

            var lookup = ourSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedRemovedAndUpdated_ReturnsTrue(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_OptimizedSet<TestType>();

            ourSet = ourSet.Add(value, tag, timestamp);
            ourSet = ourSet.Remove(value, tag, timestamp + 1);

            var newValue = Build(value.Id);

            ourSet = ourSet.Update(newValue, tag, timestamp + 1);

            var lookup = ourSet.Lookup(newValue);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Values_ReturnsNonRemovedValues(TestType one, TestType two, TestType three, Guid tagOne, Guid tagTwo, Guid tagThree, long timestamp)
        {
            var ourSet = new OUR_OptimizedSet<TestType>();

            ourSet = ourSet.Add(one, tagOne, timestamp);
            ourSet = ourSet.Add(one, tagTwo, timestamp + 1);
            ourSet = ourSet.Remove(one, tagTwo, timestamp + 2);
            ourSet = ourSet.Add(two, tagTwo, timestamp + 3);
            ourSet = ourSet.Add(two, tagOne, timestamp + 4);
            ourSet = ourSet.Remove(two, tagOne, timestamp + 5);
            ourSet = ourSet.Remove(three, tagThree, timestamp + 6);
            ourSet = ourSet.Add(three, tagThree, timestamp + 7);
            ourSet = ourSet.Remove(three, tagThree, timestamp + 8);

            var actualValues = ourSet.Values;

            Assert.Equal(2, actualValues.Count);
            Assert.Contains(one, actualValues);
            Assert.Contains(two, actualValues);
        }
    }
}
