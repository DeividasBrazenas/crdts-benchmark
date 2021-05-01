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
    public class OR_OptimizedSetTests
    {
        [Theory]
        [AutoData]
        public void Create_CreatesSetWithElements(OR_OptimizedSetElement<TestType> one, OR_OptimizedSetElement<TestType> two, 
            OR_OptimizedSetElement<TestType> three)
        {
            var elements = new[] { one, two, three }.ToImmutableHashSet();

            var orSet = new OR_OptimizedSet<TestType>(elements);

            Assert.Equal(3, orSet.Elements.Count);

            foreach (var element in elements)
            {
                Assert.Contains(element, orSet.Elements);
            }
        }

        [Theory]
        [AutoData]
        public void Add_AddsElementToAddsSet(TestType value, Guid tag)
        {
            var orSet = new OR_OptimizedSet<TestType>();

            orSet = orSet.Add(value, tag);

            var element = new OR_OptimizedSetElement<TestType>(value, tag, false);

            Assert.Contains(element, orSet.Elements);
        }

        [Theory]
        [AutoData]
        public void Add_Concurrent_AddsOnlyOneElement(TestType value, Guid tag)
        {
            var orSet = new OR_OptimizedSet<TestType>();

            orSet = orSet.Add(value, tag);
            orSet = orSet.Add(value, tag);

            var element = new OR_OptimizedSetElement<TestType>(value, tag, false);

            Assert.Equal(1, orSet.Elements.Count(v => Equals(v, element)));
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(TestType value, Guid tag)
        {
            var orSet = new OR_OptimizedSet<TestType>();

            var newOrSet = orSet.Remove(value, tag);

            Assert.Same(orSet, newOrSet);
        }

        [Theory]
        [AutoData]
        public void Remove_AddsElementToRemovesSet(TestType value, Guid tag)
        {
            var orSet = new OR_OptimizedSet<TestType>();

            orSet = orSet.Add(value, tag);
            orSet = orSet.Remove(value, tag);

            var element = new OR_OptimizedSetElement<TestType>(value, tag, true);

            Assert.Contains(element, orSet.Elements);
        }

        [Theory]
        [AutoData]
        public void Remove_Concurrent_AddsOnlyOneElementToRemoveSet(TestType value, Guid tag)
        {
            var orSet = new OR_OptimizedSet<TestType>();

            orSet = orSet.Add(value, tag);
            orSet = orSet.Remove(value, tag);
            orSet = orSet.Remove(value, tag);

            var element = new OR_OptimizedSetElement<TestType>(value, tag, true);

            Assert.Equal(1, orSet.Elements.Count(v => Equals(v, element)));
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value, Guid tag)
        {
            var orSet = new OR_OptimizedSet<TestType>();

            orSet = orSet.Add(value, tag);

            var lookup = orSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, Guid tag)
        {
            var orSet = new OR_OptimizedSet<TestType>();

            orSet = orSet.Add(value, tag);
            orSet = orSet.Remove(value, tag);

            var lookup = orSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_OptimizedSameValueWithSeveralTags_ReturnsTrue(TestType value, Guid tag)
        {
            var orSet = new OR_OptimizedSet<TestType>();

            orSet = orSet.Add(value, tag);
            orSet = orSet.Add(value, Guid.NewGuid());
            orSet = orSet.Remove(value, tag);

            var lookup = orSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Values_ReturnsNonRemovedValues(TestType one, TestType two, TestType three, Guid tagOne, Guid tagTwo, Guid tagThree)
        {
            var orSet = new OR_OptimizedSet<TestType>();

            orSet = orSet.Add(one, tagOne);
            orSet = orSet.Add(one, tagTwo);
            orSet = orSet.Remove(one, tagTwo);
            orSet = orSet.Add(two, tagThree);
            orSet = orSet.Remove(three, tagThree);
            orSet = orSet.Add(three, tagThree);
            orSet = orSet.Remove(three, tagThree);

            var actualValues = orSet.Values;

            Assert.Equal(2, actualValues.Count);
            Assert.Contains(one, actualValues);
            Assert.Contains(two, actualValues);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesAddsAndRemoves(TestType one, TestType two, TestType three, Guid tagOne, Guid tagTwo, Guid tagThree)
        {
            var orSet = new OR_OptimizedSet<TestType>(new[] { 
                    new OR_OptimizedSetElement<TestType>(one, tagOne, false), 
                    new OR_OptimizedSetElement<TestType>(two, tagOne, true),
                    new OR_OptimizedSetElement<TestType>(three, tagThree, false),
                }.ToImmutableHashSet());

            var newOrSet = orSet.Merge(new[] {
                new OR_OptimizedSetElement<TestType>(one, tagOne, false),
                new OR_OptimizedSetElement<TestType>(one, tagTwo, true),
                new OR_OptimizedSetElement<TestType>(two, tagOne, false),
                new OR_OptimizedSetElement<TestType>(three, tagThree, false),
            }.ToImmutableHashSet());

            Assert.Equal(4, newOrSet.Elements.Count);
            Assert.Contains(new OR_OptimizedSetElement<TestType>(one, tagOne, false), newOrSet.Elements);
            Assert.Contains(new OR_OptimizedSetElement<TestType>(two, tagOne, true), newOrSet.Elements);
            Assert.Contains(new OR_OptimizedSetElement<TestType>(three, tagThree, false), newOrSet.Elements);
            Assert.Contains(new OR_OptimizedSetElement<TestType>(one, tagTwo, true), newOrSet.Elements);
        }
    }
}
