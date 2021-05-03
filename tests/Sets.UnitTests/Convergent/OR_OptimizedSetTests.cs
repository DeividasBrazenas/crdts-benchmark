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
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value, Guid tag)
        {
            var orSet = new OR_OptimizedSet<TestType>();

            orSet = orSet.Merge(new []{new OR_OptimizedSetElement<TestType>(value, tag, false)}.ToImmutableHashSet());

            var lookup = orSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, Guid tag)
        {
            var orSet = new OR_OptimizedSet<TestType>();

            orSet = orSet.Merge(new[] { new OR_OptimizedSetElement<TestType>(value, tag, false) }.ToImmutableHashSet());
            orSet = orSet.Merge(new[] { new OR_OptimizedSetElement<TestType>(value, tag, true) }.ToImmutableHashSet());

            var lookup = orSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_OptimizedSameValueWithSeveralTags_ReturnsTrue(TestType value, Guid tag)
        {
            var orSet = new OR_OptimizedSet<TestType>();

            orSet = orSet.Merge(new[] { new OR_OptimizedSetElement<TestType>(value, tag, false) }.ToImmutableHashSet());
            orSet = orSet.Merge(new[] { new OR_OptimizedSetElement<TestType>(value, Guid.NewGuid(), false) }.ToImmutableHashSet());
            orSet = orSet.Merge(new[] { new OR_OptimizedSetElement<TestType>(value, tag, true) }.ToImmutableHashSet());

            var lookup = orSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Values_ReturnsNonRemovedValues(TestType one, TestType two, TestType three, Guid tagOne, Guid tagTwo, Guid tagThree)
        {
            var orSet = new OR_OptimizedSet<TestType>();

            orSet = orSet.Merge(new[] { new OR_OptimizedSetElement<TestType>(one, tagOne, false) }.ToImmutableHashSet());
            orSet = orSet.Merge(new[] { new OR_OptimizedSetElement<TestType>(one, tagOne, false) }.ToImmutableHashSet());
            orSet = orSet.Merge(new[] { new OR_OptimizedSetElement<TestType>(one, tagTwo, true) }.ToImmutableHashSet());
            orSet = orSet.Merge(new[] { new OR_OptimizedSetElement<TestType>(two, tagThree, false) }.ToImmutableHashSet());
            orSet = orSet.Merge(new[] { new OR_OptimizedSetElement<TestType>(three, tagThree, true) }.ToImmutableHashSet());
            orSet = orSet.Merge(new[] { new OR_OptimizedSetElement<TestType>(three, tagThree, false) }.ToImmutableHashSet());
            orSet = orSet.Merge(new[] { new OR_OptimizedSetElement<TestType>(three, tagThree, true) }.ToImmutableHashSet());

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
