using System;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Sets.Commutative.LastWriterWins;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
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
        public void Add_AddsElementToAddsSet(LWW_SetElement<TestType> element)
        {
            var lwwSet = new LWW_Set<TestType>();

            lwwSet = lwwSet.Add(element.Value, element.Timestamp);

            Assert.Contains(element, lwwSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_AddSameElementTwiceWithDifferentTimestamp_UpdatesExistingElement(TestType value)
        {
            var lwwSet = new LWW_Set<TestType>();

            var firstAdd = new LWW_SetElement<TestType>(value, DateTime.Now.Ticks);
            var secondAdd = new LWW_SetElement<TestType>(value, DateTime.Now.AddMinutes(1).Ticks);

            lwwSet = lwwSet.Add(firstAdd.Value, firstAdd.Timestamp);
            lwwSet = lwwSet.Add(secondAdd.Value, secondAdd.Timestamp);

            Assert.True(lwwSet.Adds.Count(e => Equals(e, secondAdd)) == 1);
            Assert.True(lwwSet.Adds.Count(e => Equals(e, firstAdd)) == 0);
        }

        [Theory]
        [AutoData]
        public void Add_ConcurrentAdds_AddsOnlyOne(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var firstAdd = new LWW_SetElement<TestType>(value, timestamp);
            var secondAdd = new LWW_SetElement<TestType>(value, timestamp);

            lwwSet = lwwSet.Add(firstAdd.Value, firstAdd.Timestamp);
            lwwSet = lwwSet.Add(secondAdd.Value, secondAdd.Timestamp);

            Assert.Equal(1, lwwSet.Adds.Count(e => Equals(e.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(LWW_SetElement<TestType> element)
        {
            var lwwSet = new LWW_Set<TestType>();

            var newLwwSet = lwwSet.Remove(element.Value, element.Timestamp);

            Assert.Same(lwwSet, newLwwSet);
        }

        [Theory]
        [AutoData]
        public void Remove_RemovesElementToRemovesSet(TestType value)
        {
            var lwwSet = new LWW_Set<TestType>();

            var add = new LWW_SetElement<TestType>(value, DateTime.Now.Ticks);
            var remove = new LWW_SetElement<TestType>(value, DateTime.Now.AddMinutes(1).Ticks);

            lwwSet = lwwSet.Add(add.Value, add.Timestamp);
            lwwSet = lwwSet.Remove(remove.Value, remove.Timestamp);

            Assert.Contains(remove, lwwSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Remove_RemoveSameElementTwiceWithDifferentTimestamp_AddsOneElement(TestType value)
        {
            var lwwSet = new LWW_Set<TestType>();

            var add = new LWW_SetElement<TestType>(value, DateTime.Now.Ticks);
            var firstRemove = new LWW_SetElement<TestType>(value, DateTime.Now.AddMinutes(1).Ticks);
            var secondRemove = new LWW_SetElement<TestType>(value, DateTime.Now.AddMinutes(2).Ticks);

            lwwSet = lwwSet.Add(add.Value, add.Timestamp);
            lwwSet = lwwSet.Remove(firstRemove.Value, firstRemove.Timestamp);
            lwwSet = lwwSet.Remove(secondRemove.Value, secondRemove.Timestamp);

            Assert.True(lwwSet.Removes.Count(e => Equals(e.Value, value)) == 1);
        }

        [Theory]
        [AutoData]
        public void Remove_ConcurrentRemoves_AddsOnlyOne(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var add = new LWW_SetElement<TestType>(value, timestamp);
            var firstRemove = new LWW_SetElement<TestType>(value, timestamp + 100);
            var secondRemove = new LWW_SetElement<TestType>(value, timestamp + 100);

            lwwSet = lwwSet.Add(add.Value, add.Timestamp);
            lwwSet = lwwSet.Remove(firstRemove.Value, firstRemove.Timestamp);
            lwwSet = lwwSet.Remove(secondRemove.Value, secondRemove.Timestamp);

            Assert.Equal(1, lwwSet.Removes.Count(e => Equals(e.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            lwwSet = lwwSet.Add(value, timestamp);

            var lookup = lwwSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var add = new LWW_SetElement<TestType>(value, timestamp);
            var remove = new LWW_SetElement<TestType>(value, timestamp + 10);

            lwwSet = lwwSet.Add(add.Value, add.Timestamp);
            lwwSet = lwwSet.Remove(remove.Value, remove.Timestamp);

            var lookup = lwwSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsTrue(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var add = new LWW_SetElement<TestType>(value, timestamp);
            var remove = new LWW_SetElement<TestType>(value, timestamp + 10);
            var reAdd = new LWW_SetElement<TestType>(value, timestamp + 100);

            lwwSet = lwwSet.Add(add.Value, add.Timestamp);
            lwwSet = lwwSet.Remove(remove.Value, remove.Timestamp);
            lwwSet = lwwSet.Add(reAdd.Value, reAdd.Timestamp);

            var lookup = lwwSet.Lookup(value);

            Assert.True(lookup);
        }
    }
}
