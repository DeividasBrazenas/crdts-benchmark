using System;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Sets.Convergent;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

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
        public void Add_Concurrent_AddsOnlyOneElement(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var element = new OUR_SetElement<TestType>(value, timestamp);

            ourSet = ourSet.Add(element);
            ourSet = ourSet.Add(element);

            Assert.Equal(1, ourSet.Adds.Count(v => Equals(v, element)));
        }

        [Theory]
        [AutoData]
        public void Add_WithSameId_AddsNewElement(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var element = new OUR_SetElement<TestType>(value, timestamp);

            ourSet = ourSet.Add(element);

            var newValue = new TestType(value.Id)
            {
                StringValue = Guid.NewGuid().ToString(),
                IntValue = value.IntValue,
                DecimalValue = 999M,
                NullableLongValue = null,
                GuidValue = Guid.Empty,
                IntArray = value.IntArray,
                LongList = value.LongList,
                ObjectValue = value.ObjectValue
            };

            var newElement = new OUR_SetElement<TestType>(newValue, timestamp + 100);

            ourSet = ourSet.Add(newElement);

            Assert.Equal(1, ourSet.Adds.Count);
            Assert.Contains(newElement, ourSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_WithSameIdInAddAndRemoveSets_AddsNewElement(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var addElement = new OUR_SetElement<TestType>(value, timestamp);
            var removeElement = new OUR_SetElement<TestType>(value, timestamp + 100);

            ourSet = ourSet.Add(addElement);
            ourSet = ourSet.Remove(removeElement);

            var newValue = new TestType(value.Id)
            {
                StringValue = Guid.NewGuid().ToString(),
                IntValue = value.IntValue,
                DecimalValue = 999M,
                NullableLongValue = null,
                GuidValue = Guid.Empty,
                IntArray = value.IntArray,
                LongList = value.LongList,
                ObjectValue = value.ObjectValue
            };

            var newElement = new OUR_SetElement<TestType>(newValue, timestamp + 200);

            ourSet = ourSet.Add(newElement);

            Assert.Equal(1, ourSet.Adds.Count);
            Assert.Equal(0, ourSet.Removes.Count);
            Assert.Contains(newElement, ourSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_WithOtherAddsAndRemoves_AddsNewElement(OUR_SetElement<TestType>[] other, 
            TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>(other.ToImmutableHashSet(), other.ToImmutableHashSet());

            var addElement = new OUR_SetElement<TestType>(value, timestamp);
            var removeElement = new OUR_SetElement<TestType>(value, timestamp + 100);

            ourSet = ourSet.Add(addElement);
            ourSet = ourSet.Remove(removeElement);

            var newValue = new TestType(value.Id)
            {
                StringValue = Guid.NewGuid().ToString(),
                IntValue = value.IntValue,
                DecimalValue = 999M,
                NullableLongValue = null,
                GuidValue = Guid.Empty,
                IntArray = value.IntArray,
                LongList = value.LongList,
                ObjectValue = value.ObjectValue
            };

            var newElement = new OUR_SetElement<TestType>(newValue, timestamp + 200);

            ourSet = ourSet.Add(newElement);

            Assert.Equal(other.Length + 1, ourSet.Adds.Count);
            Assert.Equal(other.Length, ourSet.Removes.Count);
            Assert.Contains(newElement, ourSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_WithSmallerTimestampThanInRemoveSet_AddsNewElementAndKeepsRemove(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var addElement = new OUR_SetElement<TestType>(value, timestamp);
            var removeElement = new OUR_SetElement<TestType>(value, timestamp + 200);

            ourSet = ourSet.Add(addElement);
            ourSet = ourSet.Remove(removeElement);

            var newValue = new TestType(value.Id)
            {
                StringValue = Guid.NewGuid().ToString(),
                IntValue = value.IntValue,
                DecimalValue = 999M,
                NullableLongValue = null,
                GuidValue = Guid.Empty,
                IntArray = value.IntArray,
                LongList = value.LongList,
                ObjectValue = value.ObjectValue
            };

            var newElement = new OUR_SetElement<TestType>(newValue, timestamp + 100);

            ourSet = ourSet.Add(newElement);

            Assert.Equal(1, ourSet.Adds.Count);
            Assert.Equal(1, ourSet.Removes.Count);
            Assert.Contains(newElement, ourSet.Adds);
            Assert.Contains(removeElement, ourSet.Removes);
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
        public void Value_AddedAndNotRemoved_ReturnsAddedElement(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var element = new OUR_SetElement<TestType>(value, timestamp);

            ourSet = ourSet.Add(element);

            var actualValue = ourSet.Value(value.Id);

            Assert.Equal(value, actualValue);
        }

        [Theory]
        [AutoData]
        public void Value_AddedAndRemoved_ReturnsNull(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var add = new OUR_SetElement<TestType>(value, timestamp);
            var remove = new OUR_SetElement<TestType>(value, timestamp + 100);

            ourSet = ourSet.Add(add);
            ourSet = ourSet.Remove(remove);

            var actualValue = ourSet.Value(value.Id);

            Assert.Null(actualValue);
        }

        [Theory]
        [AutoData]
        public void Value_ValueRemovedAndAdded_ReturnsAddedValue(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var addElement = new OUR_SetElement<TestType>(value, timestamp);
            var removeElement = new OUR_SetElement<TestType>(value, timestamp + 100);

            ourSet = ourSet.Add(addElement);
            ourSet = ourSet.Remove(removeElement);

            var newValue = new TestType(value.Id)
            {
                StringValue = Guid.NewGuid().ToString(),
                IntValue = value.IntValue,
                DecimalValue = 999M,
                NullableLongValue = null,
                GuidValue = Guid.Empty,
                IntArray = value.IntArray,
                LongList = value.LongList,
                ObjectValue = value.ObjectValue
            };

            var newElement = new OUR_SetElement<TestType>(newValue, timestamp + 200);

            ourSet = ourSet.Add(newElement);

            var actualValue = ourSet.Value(value.Id);

            Assert.Equal(newValue, actualValue);
        }

        [Theory]
        [AutoData]
        public void Value_Updated_ReturnsUpdatedValue(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var addElement = new OUR_SetElement<TestType>(value, timestamp);

            ourSet = ourSet.Add(addElement);

            var newValue = new TestType(value.Id)
            {
                StringValue = Guid.NewGuid().ToString(),
                IntValue = value.IntValue,
                DecimalValue = 999M,
                NullableLongValue = null,
                GuidValue = Guid.Empty,
                IntArray = value.IntArray,
                LongList = value.LongList,
                ObjectValue = value.ObjectValue
            };

            var newElement = new OUR_SetElement<TestType>(newValue, timestamp + 100);

            ourSet = ourSet.Add(newElement);

            var actualValue = ourSet.Value(value.Id);

            Assert.Equal(newValue, actualValue);
        }

        [Theory]
        [AutoData]
        public void Values_ReturnsNonRemovedValues(TestType one, TestType two, TestType three, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            ourSet = ourSet.Add(new OUR_SetElement<TestType>(one, timestamp));
            ourSet = ourSet.Add(new OUR_SetElement<TestType>(one, timestamp + 100));
            ourSet = ourSet.Add(new OUR_SetElement<TestType>(two, timestamp + 200));
            ourSet = ourSet.Remove(new OUR_SetElement<TestType>(three, timestamp + 300));
            ourSet = ourSet.Add(new OUR_SetElement<TestType>(three, timestamp + 300));
            ourSet = ourSet.Remove(new OUR_SetElement<TestType>(three, timestamp + 400));

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
            var firstPSet = new OUR_Set<TestType>(new[] { one, two }.ToImmutableHashSet(), new[] { three }.ToImmutableHashSet());

            var secondPSet = new OUR_Set<TestType>(new[] { three, four }.ToImmutableHashSet(), new[] { five }.ToImmutableHashSet());

            var ourSet = firstPSet.Merge(secondPSet);

            Assert.Equal(4, ourSet.Adds.Count);
            Assert.Equal(2, ourSet.Removes.Count);
            Assert.Contains(one, ourSet.Adds);
            Assert.Contains(two, ourSet.Adds);
            Assert.Contains(three, ourSet.Adds);
            Assert.Contains(four, ourSet.Adds);
            Assert.Contains(three, ourSet.Removes);
            Assert.Contains(five, ourSet.Removes);
            Assert.Contains(five, ourSet.Removes);
        }
    }
}
