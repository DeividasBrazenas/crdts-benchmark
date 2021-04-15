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
    public class LWW_SetTests
    {
        [Theory]
        [AutoData]
        public void Create_CreatesSetWithElements(LWW_SetElement<TestType> one, LWW_SetElement<TestType> two, LWW_SetElement<TestType> three)
        {
            var adds = new[] { one, two }.ToImmutableHashSet();
            var removes = new[] { two, three }.ToImmutableHashSet();

            var lwwSet = new LWW_Set<TestType>(adds, removes);

            Assert.Equal(adds.Count, lwwSet.Adds.Count);
            Assert.Equal(removes.Count, lwwSet.Removes.Count);

            foreach (var add in adds)
            {
                Assert.Contains(add, lwwSet.Adds);
            }

            foreach (var remove in removes)
            {
                Assert.Contains(remove, lwwSet.Removes);
            }
        }

        [Theory]
        [AutoData]
        public void Add_AddsElementToAddsSet(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);
            lwwSet = lwwSet.Add(new LWW_SetOperation(JToken.Parse(valueJson), timestamp));

            var expectedElement = new LWW_SetElement<TestType>(value, timestamp);
            Assert.Contains(expectedElement, lwwSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_AddSameElementTwiceWithDifferentTimestamp_UpdatesTimestamp(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var firstAdd = new LWW_SetOperation(JToken.Parse(valueJson), timestamp);
            var secondAdd = new LWW_SetOperation(JToken.Parse(valueJson), timestamp + 100);

            lwwSet = lwwSet.Add(firstAdd);
            lwwSet = lwwSet.Add(secondAdd);

            var expectedElement = new LWW_SetElement<TestType>(value, timestamp + 100);

            Assert.True(lwwSet.Adds.Count(e => Equals(e.Value, value)) == 1);
            Assert.Contains(expectedElement, lwwSet.Adds);

        }

        [Theory]
        [AutoData]
        public void Add_AddSameElementTwiceWithLowerTimestamp_DoesNotDoAnything(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var firstAdd = new LWW_SetOperation(JToken.Parse(valueJson), timestamp + 100);
            var secondAdd = new LWW_SetOperation(JToken.Parse(valueJson), timestamp);

            lwwSet = lwwSet.Add(firstAdd);
            lwwSet = lwwSet.Add(secondAdd);

            var expectedElement = new LWW_SetElement<TestType>(value, timestamp + 100);

            Assert.Contains(expectedElement, lwwSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_ConcurrentElements_AddsOnlyOneElement(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var firstAdd = new LWW_SetOperation(JToken.Parse(valueJson), timestamp);
            var secondAdd = new LWW_SetOperation(JToken.Parse(valueJson), timestamp);

            lwwSet = lwwSet.Add(firstAdd);
            lwwSet = lwwSet.Add(secondAdd);

            Assert.Equal(1, lwwSet.Adds.Count(e => Equals(e.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Add_WithoutId_DoesNotDoAnything(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var newValue = new
            {
                StringValue = value.StringValue,
                IntValue = value.IntValue,
                DecimalValue = value.DecimalValue,
                NullableLongValue = value.NullableLongValue,
                GuidValue = value.GuidValue,
                IntArray = value.IntArray,
                LongList = value.LongList,
                ObjectValue = value.ObjectValue
            };

            var valueJson = JsonConvert.SerializeObject(newValue);

            var add = new LWW_SetOperation(JToken.Parse(valueJson), timestamp);

            var newLwwSet = lwwSet.Add(add);

            Assert.Same(lwwSet, newLwwSet);
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var remove = new LWW_SetOperation(JToken.Parse(valueJson), timestamp);

            var newLwwSet = lwwSet.Remove(remove);

            Assert.Same(lwwSet, newLwwSet);
        }

        [Theory]
        [AutoData]
        public void Remove_RemovesElementToRemovesSet(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new LWW_SetOperation(JToken.Parse(valueJson), timestamp);
            var remove = new LWW_SetOperation(JToken.Parse(valueJson), timestamp + 100);

            lwwSet = lwwSet.Add(add);
            lwwSet = lwwSet.Remove(remove);

            var expectedRemoveElement = new LWW_SetElement<TestType>(value, timestamp + 100);

            Assert.Contains(expectedRemoveElement, lwwSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Remove_RemoveSameElementTwiceWithDifferentTimestamp_UpdatesTimestamp(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new LWW_SetOperation(JToken.Parse(valueJson), timestamp);
            var firstRemove = new LWW_SetOperation(JToken.Parse(valueJson), timestamp + 100);
            var secondRemove = new LWW_SetOperation(JToken.Parse(valueJson), timestamp + 1000);

            lwwSet = lwwSet.Add(add);
            lwwSet = lwwSet.Remove(firstRemove);
            lwwSet = lwwSet.Remove(secondRemove);

            var expectedRemoveElement = new LWW_SetElement<TestType>(value, timestamp + 1000);

            Assert.True(lwwSet.Removes.Count(e => Equals(e.Value, value)) == 1);
            Assert.Contains(expectedRemoveElement, lwwSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Remove_RemoveSameElementTwiceWithLowerTimestamp_DoesNotDoAnything(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new LWW_SetOperation(JToken.Parse(valueJson), timestamp);
            var firstRemove = new LWW_SetOperation(JToken.Parse(valueJson), timestamp + 1000);
            var secondRemove = new LWW_SetOperation(JToken.Parse(valueJson), timestamp + 100);

            lwwSet = lwwSet.Add(add);
            lwwSet = lwwSet.Remove(firstRemove);
            lwwSet = lwwSet.Remove(secondRemove);

            var expectedRemoveElement = new LWW_SetElement<TestType>(value, timestamp + 1000);

            Assert.Contains(expectedRemoveElement, lwwSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Remove_WithoutId_DoesNotDoAnything(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var newValue = new
            {
                StringValue = value.StringValue,
                IntValue = value.IntValue,
                DecimalValue = value.DecimalValue,
                NullableLongValue = value.NullableLongValue,
                GuidValue = value.GuidValue,
                IntArray = value.IntArray,
                LongList = value.LongList,
                ObjectValue = value.ObjectValue
            };

            var valueJson = JsonConvert.SerializeObject(newValue);

            var add = new LWW_SetOperation(JToken.Parse(JsonConvert.SerializeObject(value)), timestamp);
            var remove = new LWW_SetOperation(JToken.Parse(valueJson), timestamp);

            lwwSet = lwwSet.Add(add);
            var newLwwSet = lwwSet.Remove(remove);

            Assert.Same(lwwSet, newLwwSet);
        }

        [Theory]
        [AutoData]
        public void Remove_ConcurrentTimestamps_AddsOnlyOneObjectToRemoveSet(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new LWW_SetOperation(JToken.Parse(valueJson), timestamp);
            var firstRemove = new LWW_SetOperation(JToken.Parse(valueJson), timestamp + 100);
            var secondRemove = new LWW_SetOperation(JToken.Parse(valueJson), timestamp + 100);

            lwwSet = lwwSet.Add(add);
            lwwSet = lwwSet.Remove(firstRemove);
            lwwSet = lwwSet.Remove(secondRemove);

            Assert.Equal(1, lwwSet.Removes.Count(e => Equals(e.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Value_AddedAndNotRemoved_ReturnsAddedElement(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new LWW_SetOperation(JToken.Parse(valueJson), timestamp);

            lwwSet = lwwSet.Add(add);

            var actualValue = lwwSet.Value(value.Id);

            Assert.Equal(value, actualValue);
        }

        [Theory]
        [AutoData]
        public void Value_RemoveBeforeAdd_ReturnsAddedElement(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var firstAdd = new LWW_SetOperation(JToken.Parse(valueJson), timestamp);
            var secondAdd = new LWW_SetOperation(JToken.Parse(valueJson), timestamp + 1000);
            var remove = new LWW_SetOperation(JToken.Parse(valueJson), timestamp + 100);

            lwwSet = lwwSet.Add(firstAdd);
            lwwSet = lwwSet.Remove(remove);
            lwwSet = lwwSet.Add(secondAdd);

            var actualValue = lwwSet.Value(value.Id);

            Assert.Equal(value, actualValue);
        }

        [Theory]
        [AutoData]
        public void Value_RemoveAfterAdd_ReturnsNull(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new LWW_SetOperation(JToken.Parse(valueJson), timestamp);
            var remove = new LWW_SetOperation(JToken.Parse(valueJson), timestamp + 100);

            lwwSet = lwwSet.Add(add);
            lwwSet = lwwSet.Remove(remove);

            var actualValue = lwwSet.Value(value.Id);

            Assert.Null(actualValue);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesAddsAndRemoves(LWW_SetElement<TestType> one, LWW_SetElement<TestType> two,
            LWW_SetElement<TestType> three, LWW_SetElement<TestType> four, LWW_SetElement<TestType> five)
        {
            var firstLwwSet = new LWW_Set<TestType>(new[] { one, two }.ToImmutableHashSet(), new[] { three }.ToImmutableHashSet());

            var secondPSet = new LWW_Set<TestType>(new[] { three, four }.ToImmutableHashSet(), new[] { five }.ToImmutableHashSet());

            var lwwSet = firstLwwSet.Merge(secondPSet);

            Assert.Equal(4, lwwSet.Adds.Count);
            Assert.Equal(2, lwwSet.Removes.Count);
            Assert.Contains(one, lwwSet.Adds);
            Assert.Contains(two, lwwSet.Adds);
            Assert.Contains(three, lwwSet.Adds);
            Assert.Contains(four, lwwSet.Adds);
            Assert.Contains(three, lwwSet.Removes);
            Assert.Contains(five, lwwSet.Removes);
            Assert.Contains(five, lwwSet.Removes);
        }
    }
}
