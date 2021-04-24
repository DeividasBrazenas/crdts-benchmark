using System;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Sets.Commutative;
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
        public void Add_AddsElementToAddsSet(OR_SetElement<TestType> element)
        {
            var orSet = new OR_Set<TestType>();

            orSet = orSet.Add(element);

            Assert.Contains(element, orSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_Concurrent_AddsOnlyOneElement(OR_SetElement<TestType> element)
        {
            var orSet = new OR_Set<TestType>();

            orSet = orSet.Add(element);
            orSet = orSet.Add(element);

            Assert.Equal(1, orSet.Adds.Count(v => Equals(v, element)));
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(OR_SetElement<TestType> element)
        {
            var orSet = new OR_Set<TestType>();

            var newOrSet = orSet.Remove(element);

            Assert.Same(orSet, newOrSet);
        }

        [Theory]
        [AutoData]
        public void Remove_AddsElementToRemovesSet(OR_SetElement<TestType> element)
        {
            var orSet = new OR_Set<TestType>();

            orSet = orSet.Add(element);
            orSet = orSet.Remove(element);

            Assert.Contains(element, orSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Remove_Concurrent_AddsOnlyOneElementToRemoveSet(OR_SetElement<TestType> element)
        {
            var orSet = new OR_Set<TestType>();

            orSet = orSet.Add(element);
            orSet = orSet.Remove(element);
            orSet = orSet.Remove(element);

            Assert.Equal(1, orSet.Removes.Count(v => Equals(v, element)));
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value)
        {
            var orSet = new OR_Set<TestType>();

            var element = new OR_SetElement<TestType>(value, Guid.NewGuid());

            orSet = orSet.Add(element);

            var lookup = orSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value)
        {
            var orSet = new OR_Set<TestType>();

            var element = new OR_SetElement<TestType>(value, Guid.NewGuid());

            orSet = orSet.Add(element);
            orSet = orSet.Remove(element);

            var lookup = orSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SameValueWithSeveralTags_ReturnsTrue(TestType value)
        {
            var orSet = new OR_Set<TestType>();

            var elementOne = new OR_SetElement<TestType>(value, Guid.NewGuid());
            var elementTwo = new OR_SetElement<TestType>(value, Guid.NewGuid());

            orSet = orSet.Add(elementOne);
            orSet = orSet.Add(elementTwo);
            orSet = orSet.Remove(elementOne);

            var lookup = orSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Values_ReturnsNonRemovedValues(TestType one, TestType two, TestType three)
        {
            var orSet = new OR_Set<TestType>();

            var elementOne = new OR_SetElement<TestType>(one, Guid.NewGuid());
            var elementTwo = new OR_SetElement<TestType>(one, Guid.NewGuid());
            var elementThree = new OR_SetElement<TestType>(two, Guid.NewGuid());
            var elementFour = new OR_SetElement<TestType>(three, Guid.NewGuid());

            orSet = orSet.Add(elementOne);
            orSet = orSet.Add(elementTwo);
            orSet = orSet.Remove(elementTwo);
            orSet = orSet.Add(elementThree);
            orSet = orSet.Remove(elementFour);
            orSet = orSet.Add(elementFour);
            orSet = orSet.Remove(elementFour);

            var actualValues = orSet.Values;

            Assert.Equal(2, actualValues.Count);
            Assert.Contains(one, actualValues);
            Assert.Contains(two, actualValues);
        }
    }
}
