using System;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Sets.Convergent;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;
using static CRDT.UnitTestHelpers.TestTypes.TestTypeBuilder;

namespace CRDT.Sets.UnitTests.Convergent
{
    public class OUR_SetTests
    {
        [Theory]
        [AutoData]
        public void Create_CreatesSetWithElements(OUR_SetElement<TestType> one, OUR_SetElement<TestType> two,
                 OUR_SetElement<TestType> three)
        {
            var adds = new[] { one, two }.ToImmutableHashSet();
            var removes = new[] { two, three }.ToImmutableHashSet();

            var ourSet = new OUR_Set<TestType>(adds, removes);

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
        public void Add_AddsElementToAddsSet(OUR_SetElement<TestType> element)
        {
            var ourSet = new OUR_Set<TestType>();

            ourSet = ourSet.Add(element);

            Assert.Contains(element, ourSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_Concurrent_AddsOnlyOneElement(OUR_SetElement<TestType> element)
        {
            var ourSet = new OUR_Set<TestType>();

            ourSet = ourSet.Add(element);
            ourSet = ourSet.Add(element);

            Assert.Equal(1, ourSet.Adds.Count(v => Equals(v, element)));
        }

        [Theory]
        [AutoData]
        public void Update_UpdatesElementInAddsSet(OUR_SetElement<TestType> element)
        {
            var ourSet = new OUR_Set<TestType>();

            var newValue = Build(element.Value.Id);
            var newElement = new OUR_SetElement<TestType>(newValue, element.Tag, element.Timestamp.Value + 1);

            ourSet = ourSet.Add(element);
            ourSet = ourSet.Update(newElement);

            Assert.Contains(newElement, ourSet.Adds);
            Assert.DoesNotContain(element, ourSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Update_NotExistingValue_DoesNotAddToTheAddsSet(OUR_SetElement<TestType> element)
        {
            var ourSet = new OUR_Set<TestType>();

            ourSet = ourSet.Update(element);

            Assert.DoesNotContain(element, ourSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(OUR_SetElement<TestType> element)
        {
            var ourSet = new OUR_Set<TestType>();

            var newOrSet = ourSet.Remove(element);

            Assert.Same(ourSet, newOrSet);
        }

        [Theory]
        [AutoData]
        public void Remove_AddsElementToRemovesSet(OUR_SetElement<TestType> element)
        {
            var ourSet = new OUR_Set<TestType>();

            ourSet = ourSet.Add(element);
            ourSet = ourSet.Remove(element);

            Assert.Contains(element, ourSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Remove_Concurrent_AddsOnlyOneElementToRemoveSet(OUR_SetElement<TestType> element)
        {
            var ourSet = new OUR_Set<TestType>();

            ourSet = ourSet.Add(element);
            ourSet = ourSet.Remove(element);
            ourSet = ourSet.Remove(element);

            Assert.Equal(1, ourSet.Removes.Count(v => Equals(v, element)));
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var element = new OUR_SetElement<TestType>(value, Guid.NewGuid(), timestamp);

            ourSet = ourSet.Add(element);

            var lookup = ourSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var element = new OUR_SetElement<TestType>(value, Guid.NewGuid(), timestamp);

            ourSet = ourSet.Add(element);
            ourSet = ourSet.Remove(element);

            var lookup = ourSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SameValueWithSeveralTags_ReturnsTrue(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var elementOne = new OUR_SetElement<TestType>(value, Guid.NewGuid(), timestamp);
            var elementTwo = new OUR_SetElement<TestType>(value, Guid.NewGuid(), timestamp);

            ourSet = ourSet.Add(elementOne);
            ourSet = ourSet.Add(elementTwo);
            ourSet = ourSet.Remove(elementOne);

            var lookup = ourSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedRemovedAndUpdated_ReturnsTrue(OUR_SetElement<TestType> element)
        {
            var ourSet = new OUR_Set<TestType>();

            ourSet = ourSet.Add(element);
            ourSet = ourSet.Remove(element);

            var newValue = Build(element.Value.Id);
            var newElement = new OUR_SetElement<TestType>(newValue, element.Tag, element.Timestamp.Value + 1);

            ourSet = ourSet.Update(newElement);

            var lookup = ourSet.Lookup(newValue);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Values_ReturnsNonRemovedValues(TestType one, TestType two, TestType three, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var elementOne = new OUR_SetElement<TestType>(one, Guid.NewGuid(), timestamp);
            var elementTwo = new OUR_SetElement<TestType>(one, Guid.NewGuid(), timestamp);
            var elementThree = new OUR_SetElement<TestType>(two, Guid.NewGuid(), timestamp);
            var elementFour = new OUR_SetElement<TestType>(three, Guid.NewGuid(), timestamp);

            ourSet = ourSet.Add(elementOne);
            ourSet = ourSet.Add(elementTwo);
            ourSet = ourSet.Remove(elementTwo);
            ourSet = ourSet.Add(elementThree);
            ourSet = ourSet.Remove(elementFour);
            ourSet = ourSet.Add(elementFour);
            ourSet = ourSet.Remove(elementFour);

            var actualValues = ourSet.Values;

            Assert.Equal(2, actualValues.Count);
            Assert.Contains(one, actualValues);
            Assert.Contains(two, actualValues);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesAddsAndRemoves(OUR_SetElement<TestType> one, OUR_SetElement<TestType> two,
            OUR_SetElement<TestType> three, OUR_SetElement<TestType> four, OUR_SetElement<TestType> five)
        {
            var orSet = new OUR_Set<TestType>(new[] { one, two }.ToImmutableHashSet(), new[] { three }.ToImmutableHashSet());

            var newOrSet = orSet.Merge(new[] { three, four }.ToImmutableHashSet(), new[] { five }.ToImmutableHashSet());

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
