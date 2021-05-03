using System;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Sets.Convergent;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;
using static CRDT.UnitTestHelpers.TestTypes.TestTypeBuilder;

namespace CRDT.Sets.UnitTests.Convergent
{
    public class OUR_SetTests
    {
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
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            ourSet = ourSet.Merge(new[] { new OUR_SetElement<TestType>(value, tag, timestamp) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);

            var lookup = ourSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();
            ourSet = ourSet.Merge(new[] { new OUR_SetElement<TestType>(value, tag, timestamp) }.ToImmutableHashSet(), new[] { new OUR_SetElement<TestType>(value, tag, timestamp) }.ToImmutableHashSet());

            var lookup = ourSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_SameValueWithSeveralTags_ReturnsTrue(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            ourSet = ourSet.Merge(new[] { new OUR_SetElement<TestType>(value, tag, timestamp) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);
            ourSet = ourSet.Merge(new[] { new OUR_SetElement<TestType>(value, Guid.NewGuid(), timestamp + 1) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);
            ourSet = ourSet.Merge(ImmutableHashSet<OUR_SetElement<TestType>>.Empty, new[] { new OUR_SetElement<TestType>(value, tag, timestamp + 2) }.ToImmutableHashSet());

            var lookup = ourSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedRemovedAndUpdated_ReturnsTrue(TestType value, Guid tag, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            ourSet = ourSet.Merge(new[] { new OUR_SetElement<TestType>(value, tag, timestamp) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);
            ourSet = ourSet.Merge(ImmutableHashSet<OUR_SetElement<TestType>>.Empty, new[] { new OUR_SetElement<TestType>(value, tag, timestamp + 1) }.ToImmutableHashSet());

            var newValue = Build(value.Id);

            ourSet = ourSet.Merge(new[] { new OUR_SetElement<TestType>(newValue, tag, timestamp + 2) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);

            var lookup = ourSet.Lookup(newValue);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Values_ReturnsNonRemovedValues(TestType one, TestType two, TestType three, Guid tagOne, Guid tagTwo, Guid tagThree, long timestamp)
        {
            var ourSet = new OUR_Set<TestType>();

            ourSet = ourSet.Merge(new[] { new OUR_SetElement<TestType>(one, tagOne, timestamp) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);
            ourSet = ourSet.Merge(new[] { new OUR_SetElement<TestType>(one, tagOne, timestamp) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);
            ourSet = ourSet.Merge(ImmutableHashSet<OUR_SetElement<TestType>>.Empty, new[] { new OUR_SetElement<TestType>(one, tagTwo, timestamp) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_SetElement<TestType>(two, tagTwo, timestamp) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);
            ourSet = ourSet.Merge(new[] { new OUR_SetElement<TestType>(two, tagTwo, timestamp) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);
            ourSet = ourSet.Merge(new[] { new OUR_SetElement<TestType>(two, tagOne, timestamp) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);
            ourSet = ourSet.Merge(ImmutableHashSet<OUR_SetElement<TestType>>.Empty, new[] { new OUR_SetElement<TestType>(two, tagOne, timestamp) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(ImmutableHashSet<OUR_SetElement<TestType>>.Empty, new[] { new OUR_SetElement<TestType>(three, tagThree, timestamp) }.ToImmutableHashSet());
            ourSet = ourSet.Merge(new[] { new OUR_SetElement<TestType>(three, tagThree, timestamp) }.ToImmutableHashSet(), ImmutableHashSet<OUR_SetElement<TestType>>.Empty);
            ourSet = ourSet.Merge(ImmutableHashSet<OUR_SetElement<TestType>>.Empty, new[] { new OUR_SetElement<TestType>(three, tagThree, timestamp) }.ToImmutableHashSet());

            var actualValues = ourSet.Values;

            Assert.Equal(2, actualValues.Count);
            Assert.Contains(one, actualValues);
            Assert.Contains(two, actualValues);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesAddsAndRemoves(OUR_SetElement<TestType> one, OUR_SetElement<TestType> two,
            OUR_SetElement<TestType> three, OUR_SetElement<TestType> four, OUR_SetElement<TestType> five)
        {
            var ourSet = new OUR_Set<TestType>(new[] { one, two }.ToImmutableHashSet(), new[] { three }.ToImmutableHashSet());

            var newOrSet = ourSet.Merge(new[] { three, four }.ToImmutableHashSet(), new[] { five }.ToImmutableHashSet());

            Assert.Equal(4, newOrSet.Adds.Count);
            Assert.Equal(1, newOrSet.Removes.Count);
            Assert.Contains(one, newOrSet.Adds);
            Assert.Contains(two, newOrSet.Adds);
            Assert.Contains(three, newOrSet.Adds);
            Assert.Contains(four, newOrSet.Adds);
            Assert.Contains(three, newOrSet.Removes);
        }
    }
}
