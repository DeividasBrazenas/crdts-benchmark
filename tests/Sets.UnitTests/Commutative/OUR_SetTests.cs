using System;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Sets.Commutative.ObservedUpdatedRemoved;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Sets.UnitTests.Commutative
{
    public class OUR_SetTests
    {
        private readonly TestTypeBuilder _builder;

        public OUR_SetTests()
        {
            _builder = new TestTypeBuilder(new Random());
        }

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
        public void Add_AddsElementToAddsSet(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            ourSet = ourSet.Add(value, tag, timestamp);

            var element = new OUR_SetElement<TestType>(value, tag, timestamp);
            Assert.Contains(element, ourSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_Concurrent_AddsOnlyOneElement(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            ourSet = ourSet.Add(value, tag, timestamp);
            ourSet = ourSet.Add(value, tag, timestamp);

            var element = new OUR_SetElement<TestType>(value, tag, timestamp);
            Assert.Equal(1, ourSet.Adds.Count(v => Equals(v, element)));
        }

        [Theory]
        [AutoData]
        public void Update_UpdatesElementInAddsSet(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var newValue = _builder.Build(value.Id);
            var newElement = new OUR_SetElement<TestType>(newValue, tag, timestamp + 1);

            ourSet = ourSet.Add(value, tag, timestamp);
            ourSet = ourSet.Update(newValue, tag, timestamp + 1);

            var element = new OUR_SetElement<TestType>(value, tag, timestamp);

            Assert.Contains(newElement, ourSet.Adds);
            Assert.DoesNotContain(element, ourSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Update_NotExistingValue_DoesNotAddToTheAddsSet(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            ourSet = ourSet.Update(value, tag, timestamp);

            var element = new OUR_SetElement<TestType>(value, tag, timestamp);

            Assert.DoesNotContain(element, ourSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var newOrSet = ourSet.Remove(value, tag, timestamp);

            Assert.Same(ourSet, newOrSet);
        }

        [Theory]
        [AutoData]
        public void Remove_AddsElementToRemovesSet(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            ourSet = ourSet.Add(value, tag, timestamp);
            ourSet = ourSet.Remove(value, tag, timestamp);

            var element = new OUR_SetElement<TestType>(value, tag, timestamp);

            Assert.Contains(element, ourSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Remove_Concurrent_AddsOnlyOneElementToRemoveSet(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            ourSet = ourSet.Add(value, tag, timestamp);
            ourSet = ourSet.Remove(value, tag, timestamp);
            ourSet = ourSet.Remove(value, tag, timestamp);

            var element = new OUR_SetElement<TestType>(value, tag, timestamp);

            Assert.Equal(1, ourSet.Removes.Count(v => Equals(v, element)));
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            ourSet = ourSet.Add(value, tag, timestamp);

            var lookup = ourSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            var element = new OUR_SetElement<TestType>(value, Guid.NewGuid(), timestamp);

            ourSet = ourSet.Add(value, tag, timestamp);
            ourSet = ourSet.Remove(value, tag, timestamp);

            var lookup = ourSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SameValueWithSeveralTags_ReturnsTrue(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

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
            var ourSet = new OUR_Set<TestType>();

            ourSet = ourSet.Add(value, tag, timestamp);
            ourSet = ourSet.Remove(value, tag, timestamp + 1);

            var newValue = _builder.Build(value.Id);

            ourSet = ourSet.Update(newValue, tag, timestamp + 1);

            var lookup = ourSet.Lookup(newValue);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Values_ReturnsNonRemovedValues(TestType one, TestType two, TestType three, Guid tagOne, Guid tagTwo, Guid tagThree, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            ourSet = ourSet.Add(one, tagOne, timestamp);
            ourSet = ourSet.Add(one, tagTwo, timestamp);
            ourSet = ourSet.Remove(one, tagTwo, timestamp);
            ourSet = ourSet.Add(two, tagTwo, timestamp);
            ourSet = ourSet.Add(two, tagOne, timestamp);
            ourSet = ourSet.Remove(two, tagOne, timestamp);
            ourSet = ourSet.Remove(three, tagThree, timestamp);
            ourSet = ourSet.Add(three, tagThree, timestamp);
            ourSet = ourSet.Remove(three, tagThree, timestamp);

            var actualValues = ourSet.Values;

            Assert.Equal(2, actualValues.Count);
            Assert.Contains(one, actualValues);
            Assert.Contains(two, actualValues);
        }
    }
}
