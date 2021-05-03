using System;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Sets.Convergent;
using CRDT.Sets.Convergent.LastWriterWins;
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
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(LWW_SetElement<TestType> element)
        {
            var lwwSet = new LWW_Set<TestType>();

            lwwSet = lwwSet.Merge(new[] { element }.ToImmutableHashSet(), ImmutableHashSet<LWW_SetElement<TestType>>.Empty);

            var lookup = lwwSet.Lookup(element.Value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, long timestamp)
        {
            var lwwSet = new LWW_Set<TestType>();

            var add = new LWW_SetElement<TestType>(value, timestamp);
            var remove = new LWW_SetElement<TestType>(value, timestamp + 10);

            lwwSet = lwwSet.Merge(new[] { add }.ToImmutableHashSet(), new[] { remove }.ToImmutableHashSet());

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

            lwwSet = lwwSet.Merge(new[] { add, reAdd }.ToImmutableHashSet(), new[] { remove }.ToImmutableHashSet());

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
            Assert.Equal(1, newLwwSet.Removes.Count);
            Assert.Contains(one, newLwwSet.Adds);
            Assert.Contains(two, newLwwSet.Adds);
            Assert.Contains(three, newLwwSet.Adds);
            Assert.Contains(four, newLwwSet.Adds);
            Assert.Contains(three, newLwwSet.Removes);
        }
    }
}
