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

            lwwSet = lwwSet.Add(element);

            Assert.Contains(element, lwwSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_AddSameElementTwiceWithDifferentTimestamp_AddsTwoElements(TestType value)
        {
            var lwwSet = new LWW_Set<TestType>();

            var firstAdd = new LWW_SetElement<TestType>(value, DateTime.Now.Ticks);
            var secondAdd = new LWW_SetElement<TestType>(value, DateTime.Now.AddMinutes(1).Ticks);

            lwwSet = lwwSet.Add(firstAdd);
            lwwSet = lwwSet.Add(secondAdd);

            Assert.True(lwwSet.Adds.Count(e => Equals(e.Value, value)) == 2);
        }

        [Theory]
        [AutoData]
        public void Add_ConcurrentAdds_AddsOnlyOne(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var firstAdd = new LWW_SetElement<TestType>(value, timestamp);
            var secondAdd = new LWW_SetElement<TestType>(value, timestamp);

            lwwSet = lwwSet.Add(firstAdd);
            lwwSet = lwwSet.Add(secondAdd);

            Assert.Equal(1, lwwSet.Adds.Count(e => Equals(e.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(LWW_SetElement<TestType> element)
        {
            var lwwSet = new LWW_Set<TestType>();

            var newLwwSet = lwwSet.Remove(element);

            Assert.Same(lwwSet, newLwwSet);
        }

        [Theory]
        [AutoData]
        public void Remove_RemovesElementToRemovesSet(TestType value)
        {
            var lwwSet = new LWW_Set<TestType>();

            var add = new LWW_SetElement<TestType>(value, DateTime.Now.Ticks);
            var remove = new LWW_SetElement<TestType>(value, DateTime.Now.AddMinutes(1).Ticks);

            lwwSet = lwwSet.Add(add);
            lwwSet = lwwSet.Remove(remove);

            Assert.Contains(remove, lwwSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Remove_RemoveSameElementTwiceWithDifferentTimestamp_AddsTwoElements(TestType value)
        {
            var lwwSet = new LWW_Set<TestType>();

            var add = new LWW_SetElement<TestType>(value, DateTime.Now.Ticks);
            var firstRemove = new LWW_SetElement<TestType>(value, DateTime.Now.AddMinutes(1).Ticks);
            var secondRemove = new LWW_SetElement<TestType>(value, DateTime.Now.AddMinutes(2).Ticks);

            lwwSet = lwwSet.Add(add);
            lwwSet = lwwSet.Remove(firstRemove);
            lwwSet = lwwSet.Remove(secondRemove);

            Assert.True(lwwSet.Removes.Count(e => Equals(e.Value, value)) == 2);
        }

        [Theory]
        [AutoData]
        public void Remove_ConcurrentRemoves_AddsOnlyOne(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var add = new LWW_SetElement<TestType>(value, timestamp);
            var firstRemove = new LWW_SetElement<TestType>(value, timestamp + 100);
            var secondRemove = new LWW_SetElement<TestType>(value, timestamp + 100);

            lwwSet = lwwSet.Add(add);
            lwwSet = lwwSet.Remove(firstRemove);
            lwwSet = lwwSet.Remove(secondRemove);

            Assert.Equal(1, lwwSet.Removes.Count(e => Equals(e.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(LWW_SetElement<TestType> element)
        {
            var lwwSet = new LWW_Set<TestType>();

            lwwSet = lwwSet.Add(element);

            var lookup = lwwSet.Lookup(element.Value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var addElement = new LWW_SetElement<TestType>(value, timestamp);
            var removeElement = new LWW_SetElement<TestType>(value, timestamp + 10);

            lwwSet = lwwSet.Add(addElement);
            lwwSet = lwwSet.Remove(removeElement);

            var lookup = lwwSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsTrue(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var addElement = new LWW_SetElement<TestType>(value, timestamp);
            var removeElement = new LWW_SetElement<TestType>(value, timestamp + 10);
            var reAddElement = new LWW_SetElement<TestType>(value, timestamp + 100);

            lwwSet = lwwSet.Add(addElement);
            lwwSet = lwwSet.Remove(removeElement);
            lwwSet = lwwSet.Add(reAddElement);

            var lookup = lwwSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesAddsAndRemoves(LWW_SetElement<TestType> one, LWW_SetElement<TestType> two, LWW_SetElement<TestType> three, LWW_SetElement<TestType> four, LWW_SetElement<TestType> five)
        {
            var lwwSet = new LWW_Set<TestType>(new[] { one, two }.ToImmutableHashSet(), new[] { three }.ToImmutableHashSet());

            var newLwwSet = lwwSet.Merge(new[] { three, four }.ToImmutableHashSet(), new[] { five }.ToImmutableHashSet());

            Assert.Equal(4, newLwwSet.Adds.Count);
            Assert.Equal(2, newLwwSet.Removes.Count);
            Assert.Contains(one, newLwwSet.Adds);
            Assert.Contains(two, newLwwSet.Adds);
            Assert.Contains(three, newLwwSet.Adds);
            Assert.Contains(four, newLwwSet.Adds);
            Assert.Contains(three, newLwwSet.Removes);
            Assert.Contains(five, newLwwSet.Removes);
            Assert.Contains(five, newLwwSet.Removes);
        }
    }
}
