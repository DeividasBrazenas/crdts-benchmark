using System;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Sets.Commutative.ObservedRemoved;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Sets.UnitTests.Commutative
{
    public class OR_SetTests
    {
        [Theory]
        [AutoData]
        public void Create_CreatesSetWithElements(OR_SetElement<TestType> one, OR_SetElement<TestType> two,
            OR_SetElement<TestType> three)
        {
            var adds = new[] { one, two }.ToImmutableHashSet();
            var removes = new[] { two, three }.ToImmutableHashSet();

            var orSet = new OR_Set<TestType>(adds, removes);

            Assert.Equal(adds.Count, orSet.Adds.Count);
            Assert.Equal(removes.Count, orSet.Removes.Count);

            foreach (var add in adds)
            {
                Assert.Contains(add, orSet.Adds);
            }

            foreach (var remove in removes)
            {
                Assert.Contains(remove, orSet.Removes);
            }
        }

        [Theory]
        [AutoData]
        public void Add_AddsElementToAddsSet(TestType value, Guid tag)
        {
            var orSet = new OR_Set<TestType>();

            orSet = orSet.Add(value, tag);

            var element = new OR_SetElement<TestType>(value, tag);

            Assert.Contains(element, orSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_Concurrent_AddsOnlyOneElement(TestType value, Guid tag)
        {
            var orSet = new OR_Set<TestType>();

            orSet = orSet.Add(value, tag);
            orSet = orSet.Add(value, tag);

            var element = new OR_SetElement<TestType>(value, tag);

            Assert.Equal(1, orSet.Adds.Count(v => Equals(v, element)));
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(TestType value, Guid tag)
        {
            var orSet = new OR_Set<TestType>();

            var newOrSet = orSet.Remove(value, tag);

            Assert.Same(orSet, newOrSet);
        }

        [Theory]
        [AutoData]
        public void Remove_AddsElementToRemovesSet(TestType value, Guid tag)
        {
            var orSet = new OR_Set<TestType>();

            orSet = orSet.Add(value, tag);
            orSet = orSet.Remove(value, tag);

            var element = new OR_SetElement<TestType>(value, tag);

            Assert.Contains(element, orSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Remove_Concurrent_AddsOnlyOneElementToRemoveSet(TestType value, Guid tag)
        {
            var orSet = new OR_Set<TestType>();

            orSet = orSet.Add(value, tag);
            orSet = orSet.Remove(value, tag);
            orSet = orSet.Remove(value, tag);

            var element = new OR_SetElement<TestType>(value, tag);

            Assert.Equal(1, orSet.Removes.Count(v => Equals(v, element)));
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value, Guid tag)
        {
            var orSet = new OR_Set<TestType>();

            orSet = orSet.Add(value, tag);

            var lookup = orSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, Guid tag)
        {
            var orSet = new OR_Set<TestType>();

            var element = new OR_SetElement<TestType>(value, Guid.NewGuid());

            orSet = orSet.Add(value, tag);
            orSet = orSet.Remove(value, tag);

            var lookup = orSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SameValueWithSeveralTags_ReturnsTrue(TestType value, Guid tag)
        {
            var orSet = new OR_Set<TestType>();

            orSet = orSet.Add(value, tag);
            orSet = orSet.Add(value, Guid.NewGuid());
            orSet = orSet.Remove(value, tag);

            var lookup = orSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Values_ReturnsNonRemovedValues(TestType one, TestType two, TestType three, Guid tagOne, Guid tagTwo, Guid tagThree)
        {
            var orSet = new OR_Set<TestType>();

            orSet = orSet.Add(one, tagOne);
            orSet = orSet.Add(one, tagTwo);
            orSet = orSet.Remove(one, tagTwo);
            orSet = orSet.Add(two, tagTwo);
            orSet = orSet.Add(two, tagOne);
            orSet = orSet.Remove(two, tagOne);
            orSet = orSet.Remove(three, tagThree);
            orSet = orSet.Add(three, tagThree);
            orSet = orSet.Remove(three, tagThree);

            var actualValues = orSet.Values;

            Assert.Equal(2, actualValues.Count);
            Assert.Contains(one, actualValues);
            Assert.Contains(two, actualValues);
        }

    }
}
