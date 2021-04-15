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
        public void Add_AddsElementToAddsSet(LWW_SetElement<TestType>[] adds, LWW_SetElement<TestType> element)
        {
            var lwwSet = new LWW_Set<TestType>(adds.ToImmutableHashSet(), ImmutableHashSet<LWW_SetElement<TestType>>.Empty);

            lwwSet = lwwSet.Add(element);

            Assert.Contains(element, lwwSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_AddSameElementTwiceWithDifferentTimestamp_UpdatesTimestamp(LWW_SetElement<TestType>[] adds, TestType value)
        {
            var lwwSet = new LWW_Set<TestType>(adds.ToImmutableHashSet(), ImmutableHashSet<LWW_SetElement<TestType>>.Empty);

            var firstAdd = new LWW_SetElement<TestType>(value, DateTime.Now.Ticks);
            var secondAdd = new LWW_SetElement<TestType>(value, DateTime.Now.AddMinutes(1).Ticks);

            lwwSet = lwwSet.Add(firstAdd);
            lwwSet = lwwSet.Add(secondAdd);

            Assert.True(lwwSet.Adds.Count(e => Equals(e.Value, value)) == 1);
            Assert.Contains(secondAdd, lwwSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_AddSameElementTwiceWithLowerTimestamp_DoesNotDoAnything(LWW_SetElement<TestType>[] adds, TestType value)
        {
            var lwwSet = new LWW_Set<TestType>(adds.ToImmutableHashSet(), ImmutableHashSet<LWW_SetElement<TestType>>.Empty);

            var firstAdd = new LWW_SetElement<TestType>(value, DateTime.Now.AddMinutes(1).Ticks);
            var secondAdd = new LWW_SetElement<TestType>(value, DateTime.Now.Ticks);

            lwwSet = lwwSet.Add(firstAdd);
            lwwSet = lwwSet.Add(secondAdd);

            Assert.Contains(firstAdd, lwwSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(LWW_SetElement<TestType>[] removes, LWW_SetElement<TestType> element)
        {
            var lwwSet = new LWW_Set<TestType>(ImmutableHashSet<LWW_SetElement<TestType>>.Empty, removes.ToImmutableHashSet());

            var newLwwSet = lwwSet.Remove(element);

            Assert.Same(lwwSet, newLwwSet);
        }

        [Theory]
        [AutoData]
        public void Remove_RemovesElementToRemovesSet(LWW_SetElement<TestType>[] adds,
            LWW_SetElement<TestType>[] removes, TestType value)
        {
            var lwwSet = new LWW_Set<TestType>(adds.ToImmutableHashSet(), removes.ToImmutableHashSet());

            var add = new LWW_SetElement<TestType>(value, DateTime.Now.Ticks);
            var remove = new LWW_SetElement<TestType>(value, DateTime.Now.AddMinutes(1).Ticks);

            lwwSet = lwwSet.Add(add);
            lwwSet = lwwSet.Remove(remove);

            Assert.Contains(add, lwwSet.Adds);
            Assert.Contains(remove, lwwSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Remove_RemoveSameElementTwiceWithDifferentTimestamp_UpdatesTimestamp(LWW_SetElement<TestType>[] adds,
            LWW_SetElement<TestType>[] removes, TestType value)
        {
            var lwwSet = new LWW_Set<TestType>(adds.ToImmutableHashSet(), removes.ToImmutableHashSet());

            var add = new LWW_SetElement<TestType>(value, DateTime.Now.Ticks);
            var firstRemove = new LWW_SetElement<TestType>(value, DateTime.Now.AddMinutes(1).Ticks);
            var secondRemove = new LWW_SetElement<TestType>(value, DateTime.Now.AddMinutes(2).Ticks);

            lwwSet = lwwSet.Add(add);
            lwwSet = lwwSet.Remove(firstRemove);
            lwwSet = lwwSet.Remove(secondRemove);

            Assert.Contains(add, lwwSet.Adds);
            Assert.True(lwwSet.Removes.Count(e => Equals(e.Value, value)) == 1);
            Assert.Contains(secondRemove, lwwSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Remove_RemoveSameElementTwiceWithLowerTimestamp_DoesNotDoAnything(LWW_SetElement<TestType>[] adds,
            LWW_SetElement<TestType>[] removes, TestType value)
        {
            var lwwSet = new LWW_Set<TestType>(adds.ToImmutableHashSet(), removes.ToImmutableHashSet());

            var add = new LWW_SetElement<TestType>(value, DateTime.Now.Ticks);
            var firstRemove = new LWW_SetElement<TestType>(value, DateTime.Now.AddMinutes(2).Ticks);
            var secondRemove = new LWW_SetElement<TestType>(value, DateTime.Now.AddMinutes(1).Ticks);

            lwwSet = lwwSet.Add(add);
            lwwSet = lwwSet.Remove(firstRemove);
            lwwSet = lwwSet.Remove(secondRemove);

            Assert.Contains(add, lwwSet.Adds);
            Assert.Contains(firstRemove, lwwSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Value_AddedAndNotRemoved_ReturnsAddedElement(LWW_SetElement<TestType>[] adds,
            LWW_SetElement<TestType>[] removes, LWW_SetElement<TestType> element)
        {
            var lwwSet = new LWW_Set<TestType>(adds.ToImmutableHashSet(), removes.ToImmutableHashSet());

            lwwSet = lwwSet.Add(element);

            var actualValue = lwwSet.Value(element.Value.Id);

            Assert.Equal(element.Value, actualValue);
        }

        [Theory]
        [AutoData]
        public void Value_RemoveBeforeAdd_ReturnsAddedElement(LWW_SetElement<TestType>[] adds,
            LWW_SetElement<TestType>[] removes, TestType value)
        {
            var lwwSet = new LWW_Set<TestType>(adds.ToImmutableHashSet(), removes.ToImmutableHashSet());

            var firstAdd = new LWW_SetElement<TestType>(value, DateTime.Now.Ticks);
            var secondAdd = new LWW_SetElement<TestType>(value, DateTime.Now.AddMinutes(2).Ticks);
            var remove = new LWW_SetElement<TestType>(value, DateTime.Now.AddMinutes(1).Ticks);

            lwwSet = lwwSet.Add(firstAdd);
            lwwSet = lwwSet.Remove(remove);
            lwwSet = lwwSet.Add(secondAdd);

            var actualValue = lwwSet.Value(value.Id);

            Assert.Equal(value, actualValue);
        }

        [Theory]
        [AutoData]
        public void Value_RemoveAfterAdd_ReturnsNull(LWW_SetElement<TestType>[] adds,
            LWW_SetElement<TestType>[] removes, TestType value)
        {
            var lwwSet = new LWW_Set<TestType>(adds.ToImmutableHashSet(), removes.ToImmutableHashSet());

            var add = new LWW_SetElement<TestType>(value, DateTime.Now.Ticks);
            var remove = new LWW_SetElement<TestType>(value, DateTime.Now.AddMinutes(1).Ticks);

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
