using System;
using System.Collections.Immutable;
using AutoFixture.Xunit2;
using CRDT.Sets.Convergent.ObservedUpdatedRemoved;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;
using static CRDT.UnitTestHelpers.TestTypes.TestTypeBuilder;

namespace CRDT.Sets.UnitTests.Convergent
{
    public class OUR_OptimizedSetTests
    {
        [Theory]
        [AutoData]
        public void Create_CreatesSetWithElements(OUR_OptimizedSetElement<TestType> one, OUR_OptimizedSetElement<TestType> two,
                 OUR_OptimizedSetElement<TestType> three)
        {
            var elements = new[] { one, two, three }.ToImmutableHashSet();

            var ourSet = new OUR_OptimizedSet<TestType>(elements);

            Assert.Equal(elements.Count, ourSet.Elements.Count);

            foreach (var add in elements)
            {
                Assert.Contains(add, ourSet.Elements);
            }
        }


        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_OptimizedSet<TestType>();

            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, false) }.ToImmutableHashSet());

            var lookup = ourSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_OptimizedSet<TestType>();
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, false) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetElement<TestType>(value, tag, timestamp + 1, true) }.ToImmutableHashSet());

            var lookup = ourSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SameValueWithSeveralTags_ReturnsTrue(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_OptimizedSet<TestType>();

            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, false) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetElement<TestType>(value, Guid.NewGuid(), timestamp, false) }.ToImmutableHashSet());

            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetElement<TestType>(value, tag, timestamp + 2, true) }.ToImmutableHashSet());

            var lookup = ourSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedRemovedAndUpdated_ReturnsTrue(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_OptimizedSet<TestType>();

            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetElement<TestType>(value, tag, timestamp, false) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetElement<TestType>(value, tag, timestamp + 1, true) }.ToImmutableHashSet());
          
            var newValue = Build(value.Id);

            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetElement<TestType>(newValue, tag, timestamp + 3, false) }.ToImmutableHashSet());

            var lookup = ourSet.Lookup(newValue);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Values_ReturnsNonRemovedValues(TestType one, TestType two, TestType three, Guid tagOne, Guid tagTwo, Guid tagThree, long timestamp)
        {
            var ourSet = new OUR_OptimizedSet<TestType>();

            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetElement<TestType>(one, tagOne, timestamp, false) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetElement<TestType>(one, tagOne, timestamp + 1, false) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetElement<TestType>(one, tagTwo, timestamp + 2, true) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetElement<TestType>(two, tagTwo, timestamp + 3, false) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetElement<TestType>(two, tagTwo, timestamp + 4, false) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetElement<TestType>(two, tagOne, timestamp + 5, false) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetElement<TestType>(two, tagOne, timestamp + 6, true) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetElement<TestType>(three, tagThree, timestamp + 7, true) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetElement<TestType>(three, tagThree, timestamp + 8, false) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_OptimizedSetElement<TestType>(three, tagThree, timestamp + 9, true) }.ToImmutableHashSet());

            var actualValues = ourSet.Values;

            Assert.Equal(2, actualValues.Count);
            Assert.Contains(one, actualValues);
            Assert.Contains(two, actualValues);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesAddsAndRemoves(TestType one, TestType two, TestType three, Guid tagOne, Guid tagTwo, Guid tagThree, long timestamp)
        {
            var orSet = new OUR_OptimizedSet<TestType>(new[] {
                new OUR_OptimizedSetElement<TestType>(one, tagOne, timestamp + 1, false),
                new OUR_OptimizedSetElement<TestType>(two, tagOne, timestamp + 7, true),
                new OUR_OptimizedSetElement<TestType>(three, tagThree, timestamp + 3, false),
            }.ToImmutableHashSet());

            var newOrSet = orSet.Merge(new[] {
                new OUR_OptimizedSetElement<TestType>(one, tagOne, timestamp + 4, false),
                new OUR_OptimizedSetElement<TestType>(one, tagTwo, timestamp + 5, true),
                new OUR_OptimizedSetElement<TestType>(two, tagOne, timestamp + 6, false),
                new OUR_OptimizedSetElement<TestType>(three, tagThree, timestamp + 7, false),
            }.ToImmutableHashSet());

            Assert.Equal(4, newOrSet.Elements.Count);
            Assert.Contains(new OUR_OptimizedSetElement<TestType>(one, tagOne, timestamp + 4, false), newOrSet.Elements);
            Assert.Contains(new OUR_OptimizedSetElement<TestType>(two, tagOne, timestamp + 7, true), newOrSet.Elements);
            Assert.Contains(new OUR_OptimizedSetElement<TestType>(three, tagThree, timestamp + 7, false), newOrSet.Elements);
            Assert.Contains(new OUR_OptimizedSetElement<TestType>(one, tagTwo, timestamp + 5, true), newOrSet.Elements);
        }
    }
}
