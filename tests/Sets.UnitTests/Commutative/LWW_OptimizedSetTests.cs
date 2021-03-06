using System;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Sets.Commutative.LastWriterWins;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Sets.UnitTests.Commutative
{
    public class LWW_OptimizedSetTests
    {
        [Theory]
        [AutoData]
        public void Add_AddsElementToAddsSet(TestType value)
        {
            var lwwSet = new LWW_OptimizedSet<TestType>();

            var add = new LWW_OptimizedSetElement<TestType>(value, DateTime.Now.Ticks, false);

            lwwSet = lwwSet.Assign(add.Value, add.Timestamp);

            Assert.Contains(add, lwwSet.Elements);
        }

        [Theory]
        [AutoData]
        public void Add_AddSameElementTwiceWithDifferentTimestamp_UpdatesTimestamp(TestType value)
        {
            var lwwSet = new LWW_OptimizedSet<TestType>();

            var firstAdd = new LWW_OptimizedSetElement<TestType>(value, DateTime.Now.Ticks, false);
            var secondAdd = new LWW_OptimizedSetElement<TestType>(value, DateTime.Now.AddMinutes(1).Ticks, false);

            lwwSet = lwwSet.Assign(firstAdd.Value, firstAdd.Timestamp);
            lwwSet = lwwSet.Assign(secondAdd.Value, secondAdd.Timestamp);

            Assert.True(lwwSet.Elements.Count(e => Equals(e, firstAdd)) == 0);
            Assert.True(lwwSet.Elements.Count(e => Equals(e, secondAdd)) == 1);
        }

        [Theory]
        [AutoData]
        public void Add_ConcurrentAdds_AddsOnlyOne(TestType value, long timestamp)
        {
            var lwwSet = new LWW_OptimizedSet<TestType>();

            var firstAdd = new LWW_OptimizedSetElement<TestType>(value, timestamp, false);
            var secondAdd = new LWW_OptimizedSetElement<TestType>(value, timestamp, false);

            lwwSet = lwwSet.Assign(firstAdd.Value, firstAdd.Timestamp);
            lwwSet = lwwSet.Assign(secondAdd.Value, secondAdd.Timestamp);

            Assert.Equal(1, lwwSet.Elements.Count(e => Equals(e.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(LWW_OptimizedSetElement<TestType> element)
        {
            var lwwSet = new LWW_OptimizedSet<TestType>();

            var newLwwSet = lwwSet.Remove(element.Value, element.Timestamp);

            Assert.Same(lwwSet, newLwwSet);
        }

        [Theory]
        [AutoData]
        public void Remove_RemovesElementToRemovesSet(TestType value)
        {
            var lwwSet = new LWW_OptimizedSet<TestType>();

            var add = new LWW_OptimizedSetElement<TestType>(value, DateTime.Now.Ticks, false);
            var remove = new LWW_OptimizedSetElement<TestType>(value, DateTime.Now.AddMinutes(1).Ticks, true);

            lwwSet = lwwSet.Assign(add.Value, add.Timestamp);
            lwwSet = lwwSet.Remove(remove.Value, remove.Timestamp);

            Assert.DoesNotContain(add, lwwSet.Elements);
            Assert.Contains(remove, lwwSet.Elements);
        }

        [Theory]
        [AutoData]
        public void Remove_RemoveSameElementTwiceWithDifferentTimestamp_AddsOneElements(TestType value)
        {
            var lwwSet = new LWW_OptimizedSet<TestType>();

            var add = new LWW_OptimizedSetElement<TestType>(value, DateTime.Now.Ticks, false);
            var firstRemove = new LWW_OptimizedSetElement<TestType>(value, DateTime.Now.AddMinutes(1).Ticks, true);
            var secondRemove = new LWW_OptimizedSetElement<TestType>(value, DateTime.Now.AddMinutes(2).Ticks, true);

            lwwSet = lwwSet.Assign(add.Value, add.Timestamp);
            lwwSet = lwwSet.Remove(firstRemove.Value, firstRemove.Timestamp);
            lwwSet = lwwSet.Remove(secondRemove.Value, secondRemove.Timestamp);

            Assert.True(lwwSet.Elements.Count(e => Equals(e.Value, value)) == 1);
        }

        [Theory]
        [AutoData]
        public void Remove_ConcurrentRemoves_AddsOnlyOne(TestType value, long timestamp)
        {
            var lwwSet = new LWW_OptimizedSet<TestType>();

            var add = new LWW_OptimizedSetElement<TestType>(value, timestamp, false);
            var firstRemove = new LWW_OptimizedSetElement<TestType>(value, timestamp + 100, true);
            var secondRemove = new LWW_OptimizedSetElement<TestType>(value, timestamp + 100, true);

            lwwSet = lwwSet.Assign(add.Value, add.Timestamp);
            lwwSet = lwwSet.Remove(firstRemove.Value, firstRemove.Timestamp);
            lwwSet = lwwSet.Remove(secondRemove.Value, secondRemove.Timestamp);

            Assert.Equal(1, lwwSet.Elements.Count(e => Equals(e.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(LWW_OptimizedSetElement<TestType> element)
        {
            var lwwSet = new LWW_OptimizedSet<TestType>();

            lwwSet = lwwSet.Assign(element.Value, element.Timestamp);

            var lookup = lwwSet.Lookup(element.Value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, long timestamp)
        {
            var lwwSet = new LWW_OptimizedSet<TestType>();

            var add = new LWW_OptimizedSetElement<TestType>(value, timestamp, false);
            var remove = new LWW_OptimizedSetElement<TestType>(value, timestamp + 10, true);

            lwwSet = lwwSet.Assign(add.Value, add.Timestamp);
            lwwSet = lwwSet.Remove(remove.Value, remove.Timestamp);

            var lookup = lwwSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsTrue(TestType value, long timestamp)
        {
            var lwwSet = new LWW_OptimizedSet<TestType>();

            var add = new LWW_OptimizedSetElement<TestType>(value, timestamp, false);
            var remove = new LWW_OptimizedSetElement<TestType>(value, timestamp + 10, true);
            var reAdd = new LWW_OptimizedSetElement<TestType>(value, timestamp + 100, false);

            lwwSet = lwwSet.Assign(add.Value, add.Timestamp);
            lwwSet = lwwSet.Remove(remove.Value, remove.Timestamp);
            lwwSet = lwwSet.Assign(reAdd.Value, reAdd.Timestamp);

            var lookup = lwwSet.Lookup(value);

            Assert.True(lookup);
        }
    }
}
