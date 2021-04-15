using System;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Sets.Commutative;
using CRDT.Sets.Entities;
using CRDT.Sets.Operations;
using CRDT.UnitTestHelpers.TestTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CRDT.Sets.UnitTests.Commutative
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
        public void Add_AddsElementToAddsSet(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new OUR_SetOperation(JToken.Parse(valueJson), timestamp);

            ourSet = ourSet.Add(add);

            var expectedElement = new OUR_SetElement<TestType>(value, timestamp);

            Assert.Contains(expectedElement, ourSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_Concurrent_AddsOnlyOneElement(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new OUR_SetOperation(JToken.Parse(valueJson), timestamp);

            ourSet = ourSet.Add(add);
            ourSet = ourSet.Add(add);

            var expectedElement = new OUR_SetElement<TestType>(value, timestamp);

            Assert.Equal(1, ourSet.Adds.Count(v => Equals(v, expectedElement)));
        }

        [Theory]
        [AutoData]
        public void Add_WithSameId_AddsNewElement(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new OUR_SetOperation(JToken.Parse(valueJson), timestamp);

            ourSet = ourSet.Add(add);

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

            var newValueJson = JsonConvert.SerializeObject(newValue);

            var newAdd = new OUR_SetOperation(JToken.Parse(newValueJson), timestamp + 100);

            ourSet = ourSet.Add(newAdd);

            var expectedElement = new OUR_SetElement<TestType>(newValue, timestamp + 100);

            Assert.Equal(1, ourSet.Adds.Count);
            Assert.Contains(expectedElement, ourSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_WithSameIdInAddAndRemoveSets_AddsNewElement(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new OUR_SetOperation(JToken.Parse(valueJson), timestamp);
            var remove = new OUR_SetOperation(JToken.Parse(valueJson), timestamp + 100);

            ourSet = ourSet.Add(add);
            ourSet = ourSet.Remove(remove);

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

            var newValueJson = JsonConvert.SerializeObject(newValue);

            var newAdd = new OUR_SetOperation(JToken.Parse(newValueJson), timestamp + 200);

            ourSet = ourSet.Add(newAdd);

            var expectedElement = new OUR_SetElement<TestType>(newValue, timestamp + 200);

            Assert.Equal(1, ourSet.Adds.Count);
            Assert.Equal(0, ourSet.Removes.Count);
            Assert.Contains(expectedElement, ourSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_WithOtherAddsAndRemoves_AddsNewElement(OUR_SetElement<TestType>[] other,
            TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>(other.ToImmutableHashSet(), other.ToImmutableHashSet());

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new OUR_SetOperation(JToken.Parse(valueJson), timestamp);
            var remove = new OUR_SetOperation(JToken.Parse(valueJson), timestamp + 100);

            ourSet = ourSet.Add(add);
            ourSet = ourSet.Remove(remove);

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

            var newValueJson = JsonConvert.SerializeObject(newValue);

            var newAdd = new OUR_SetOperation(JToken.Parse(newValueJson), timestamp + 200);

            ourSet = ourSet.Add(newAdd);

            var expectedElement = new OUR_SetElement<TestType>(newValue, timestamp + 200);

            Assert.Equal(other.Length + 1, ourSet.Adds.Count);
            Assert.Equal(other.Length, ourSet.Removes.Count);
            Assert.Contains(expectedElement, ourSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_WithSmallerTimestampThanInRemoveSet_AddsNewElementAndKeepsRemove(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new OUR_SetOperation(JToken.Parse(valueJson), timestamp);
            var remove = new OUR_SetOperation(JToken.Parse(valueJson), timestamp + 200);

            ourSet = ourSet.Add(add);
            ourSet = ourSet.Remove(remove);

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

            var newValueJson = JsonConvert.SerializeObject(newValue);

            var newAdd = new OUR_SetOperation(JToken.Parse(newValueJson), timestamp + 100);

            ourSet = ourSet.Add(newAdd);

            var expectedAddElement = new OUR_SetElement<TestType>(newValue, timestamp + 100);
            var expectedRemoveElement = new OUR_SetElement<TestType>(value, timestamp + 200);

            Assert.Equal(1, ourSet.Adds.Count);
            Assert.Equal(1, ourSet.Removes.Count);
            Assert.Contains(expectedAddElement, ourSet.Adds);
            Assert.Contains(expectedRemoveElement, ourSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var remove = new OUR_SetOperation(JToken.Parse(valueJson), timestamp);

            var newOrSet = ourSet.Remove(remove);

            Assert.Same(ourSet, newOrSet);
        }

        [Theory]
        [AutoData]
        public void Remove_AddsElementToRemovesSet(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new OUR_SetOperation(JToken.Parse(valueJson), timestamp);
            var remove = new OUR_SetOperation(JToken.Parse(valueJson), timestamp + 200);

            ourSet = ourSet.Add(add);
            ourSet = ourSet.Remove(remove);

            var expectedElement = new OUR_SetElement<TestType>(value, timestamp + 200);

            Assert.Contains(expectedElement, ourSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Remove_Concurrent_AddsOnlyOneElementToRemoveSet(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new OUR_SetOperation(JToken.Parse(valueJson), timestamp);
            var remove = new OUR_SetOperation(JToken.Parse(valueJson), timestamp + 200);

            ourSet = ourSet.Add(add);
            ourSet = ourSet.Remove(remove);
            ourSet = ourSet.Remove(remove);

            var expectedElement = new OUR_SetElement<TestType>(value, timestamp + 200);

            Assert.Equal(1, ourSet.Removes.Count(v => Equals(v, expectedElement)));
        }

        [Theory]
        [AutoData]
        public void Value_AddedAndNotRemoved_ReturnsAddedElement(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new OUR_SetOperation(JToken.Parse(valueJson), timestamp);

            ourSet = ourSet.Add(add);

            var actualValue = ourSet.Value(value.Id);

            Assert.Equal(value, actualValue);
        }

        [Theory]
        [AutoData]
        public void Value_AddedAndRemoved_ReturnsNull(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new OUR_SetOperation(JToken.Parse(valueJson), timestamp);
            var remove = new OUR_SetOperation(JToken.Parse(valueJson), timestamp + 100);

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

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new OUR_SetOperation(JToken.Parse(valueJson), timestamp);
            var remove = new OUR_SetOperation(JToken.Parse(valueJson), timestamp + 100);

            ourSet = ourSet.Add(add);
            ourSet = ourSet.Remove(remove);

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

            var newValueJson = JsonConvert.SerializeObject(newValue);

            var newAdd = new OUR_SetOperation(JToken.Parse(newValueJson), timestamp + 200);

            ourSet = ourSet.Add(newAdd);

            var actualValue = ourSet.Value(value.Id);

            Assert.Equal(newValue, actualValue);
        }

        [Theory]
        [AutoData]
        public void Value_Updated_ReturnsUpdatedValue(TestType value, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new OUR_SetOperation(JToken.Parse(valueJson), timestamp);

            ourSet = ourSet.Add(add);

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

            var newValueJson = JsonConvert.SerializeObject(newValue);

            var newAdd = new OUR_SetOperation(JToken.Parse(newValueJson), timestamp + 100);

            ourSet = ourSet.Add(newAdd);

            var actualValue = ourSet.Value(value.Id);

            Assert.Equal(newValue, actualValue);
        }

        [Theory]
        [AutoData]
        public void Values_ReturnsNonRemovedValues(TestType one, TestType two, TestType three, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var oneJson = JsonConvert.SerializeObject(one);
            var twoJson = JsonConvert.SerializeObject(two);
            var threeJson = JsonConvert.SerializeObject(three);

            ourSet = ourSet.Add(new OUR_SetOperation(JToken.Parse(oneJson), timestamp));
            ourSet = ourSet.Add(new OUR_SetOperation(JToken.Parse(oneJson), timestamp + 100));
            ourSet = ourSet.Add(new OUR_SetOperation(JToken.Parse(twoJson), timestamp + 200));
            ourSet = ourSet.Remove(new OUR_SetOperation(JToken.Parse(threeJson), timestamp + 300));
            ourSet = ourSet.Add(new OUR_SetOperation(JToken.Parse(threeJson), timestamp + 300));
            ourSet = ourSet.Remove(new OUR_SetOperation(JToken.Parse(threeJson), timestamp + 400));

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
