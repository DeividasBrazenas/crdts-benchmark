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

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new OR_SetOperation(JToken.Parse(valueJson), tag);

            orSet = orSet.Add(add);

            var expectedElement = new OR_SetElement<TestType>(value, tag);
            Assert.Contains(expectedElement, orSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_Concurrent_AddsOnlyOneElement(TestType value, Guid tag)
        {
            var orSet = new OR_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new OR_SetOperation(JToken.Parse(valueJson), tag);

            orSet = orSet.Add(add);
            orSet = orSet.Add(add);

            var expectedElement = new OR_SetElement<TestType>(value, tag);
            Assert.Equal(1, orSet.Adds.Count(v => Equals(v, expectedElement)));
        }

        [Theory]
        [AutoData]
        public void Add_DifferentTags_AddsSeveralElements(TestType value, Guid firstTag, Guid secondTag)
        {
            var orSet = new OR_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var firstAdd = new OR_SetOperation(JToken.Parse(valueJson), firstTag);
            var secondAdd = new OR_SetOperation(JToken.Parse(valueJson), secondTag);

            orSet = orSet.Add(firstAdd);
            orSet = orSet.Add(secondAdd);

            var expectedFirstElement = new OR_SetElement<TestType>(value, firstTag);
            var expectedSecondElement = new OR_SetElement<TestType>(value, secondTag);

            Assert.Equal(1, orSet.Adds.Count(v => Equals(v, expectedFirstElement)));
            Assert.Equal(1, orSet.Adds.Count(v => Equals(v, expectedSecondElement)));
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(TestType value, Guid tag)
        {
            var orSet = new OR_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var remove = new OR_SetOperation(JToken.Parse(valueJson), tag);

            var newOrSet = orSet.Remove(remove);

            Assert.Same(orSet, newOrSet);
        }

        [Theory]
        [AutoData]
        public void Remove_AddsElementToRemovesSet(TestType value, Guid tag)
        {
            var orSet = new OR_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new OR_SetOperation(JToken.Parse(valueJson), tag);
            var remove = new OR_SetOperation(JToken.Parse(valueJson), tag);

            orSet = orSet.Add(add);
            orSet = orSet.Remove(remove);

            var expectedElement = new OR_SetElement<TestType>(value, tag);
            Assert.Equal(1, orSet.Removes.Count(v => Equals(v, expectedElement)));
        }

        [Theory]
        [AutoData]
        public void Remove_Concurrent_AddsOnlyOneElementToRemoveSet(TestType value, Guid tag)
        {
            var orSet = new OR_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new OR_SetOperation(JToken.Parse(valueJson), tag);
            var remove = new OR_SetOperation(JToken.Parse(valueJson), tag);

            orSet = orSet.Add(add);
            orSet = orSet.Remove(remove);
            orSet = orSet.Remove(remove);

            var expectedElement = new OR_SetElement<TestType>(value, tag);
            Assert.Equal(1, orSet.Removes.Count(v => Equals(v, expectedElement)));
        }

        [Theory]
        [AutoData]
        public void Value_AddedAndNotRemoved_ReturnsAddedElement(TestType value, Guid tag)
        {
            var orSet = new OR_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new OR_SetOperation(JToken.Parse(valueJson), tag);

            orSet = orSet.Add(add);

            var actualValue = orSet.Value(value.Id);

            Assert.Equal(value, actualValue);
        }

        [Theory]
        [AutoData]
        public void Value_AddedAndRemoved_ReturnsNull(TestType value, Guid tag)
        {
            var orSet = new OR_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var add = new OR_SetOperation(JToken.Parse(valueJson), tag);
            var remove = new OR_SetOperation(JToken.Parse(valueJson), tag);

            orSet = orSet.Add(add);
            orSet = orSet.Remove(remove);

            var actualValue = orSet.Value(value.Id);

            Assert.Null(actualValue);
        }

        [Theory]
        [AutoData]
        public void Value_SameValueWithSeveralTags_ReturnsAddedValue(TestType value, Guid firstTag, Guid secondTag)
        {
            var orSet = new OR_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var firstAdd = new OR_SetOperation(JToken.Parse(valueJson), firstTag);
            var secondAdd = new OR_SetOperation(JToken.Parse(valueJson), secondTag);
            var remove = new OR_SetOperation(JToken.Parse(valueJson), firstTag);

            orSet = orSet.Add(firstAdd);
            orSet = orSet.Add(secondAdd);
            orSet = orSet.Remove(remove);

            var actualValue = orSet.Value(value.Id);

            Assert.Equal(value, actualValue);
        }

        [Theory]
        [AutoData]
        public void Values_ReturnsNonRemovedValues(TestType one, TestType two, TestType three)
        {
            var orSet = new OR_Set<TestType>();

            var valueOneJson = JsonConvert.SerializeObject(one);
            var valueTwoJson = JsonConvert.SerializeObject(two);
            var valueThreeJson = JsonConvert.SerializeObject(three);

            var first = new OR_SetOperation(JToken.Parse(valueOneJson), Guid.NewGuid());
            var second = new OR_SetOperation(JToken.Parse(valueOneJson), Guid.NewGuid());
            var third = new OR_SetOperation(JToken.Parse(valueTwoJson), Guid.NewGuid());
            var fourth = new OR_SetOperation(JToken.Parse(valueThreeJson), Guid.NewGuid());

            orSet = orSet.Add(first);
            orSet = orSet.Add(second);
            orSet = orSet.Remove(second);
            orSet = orSet.Add(third);
            orSet = orSet.Remove(fourth);
            orSet = orSet.Add(fourth);
            orSet = orSet.Remove(fourth);

            var actualValues = orSet.Values;

            Assert.Equal(2, actualValues.Count);
            Assert.Contains(one, actualValues);
            Assert.Contains(two, actualValues);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesAddsAndRemoves(OR_SetElement<TestType> one, OR_SetElement<TestType> two, 
            OR_SetElement<TestType> three, OR_SetElement<TestType> four, OR_SetElement<TestType> five)
        {
            var firstPSet = new OR_Set<TestType>(new[] { one, two }.ToImmutableHashSet(), new[] { three }.ToImmutableHashSet());

            var secondPSet = new OR_Set<TestType>(new[] { three, four }.ToImmutableHashSet(), new[] { five }.ToImmutableHashSet());

            var orSet = firstPSet.Merge(secondPSet);

            Assert.Equal(4, orSet.Adds.Count);
            Assert.Equal(2, orSet.Removes.Count);
            Assert.Contains(one, orSet.Adds);
            Assert.Contains(two, orSet.Adds);
            Assert.Contains(three, orSet.Adds);
            Assert.Contains(four, orSet.Adds);
            Assert.Contains(three, orSet.Removes);
            Assert.Contains(five, orSet.Removes);
            Assert.Contains(five, orSet.Removes);
        }
    }
}
