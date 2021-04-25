using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Sets.Convergent;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Sets.UnitTests.Convergent
{
    public class P_SetTests
    {
        [Theory]
        [AutoData]
        public void Create_CreatesSetWithElements(TestType one, TestType two, TestType three)
        {
            var adds = new[] { one, two }.ToImmutableHashSet();
            var removes = new[] { two, three }.ToImmutableHashSet();

            var pSet = new P_Set<TestType>(adds, removes);

            Assert.Equal(adds.Count, pSet.Adds.Count);
            Assert.Equal(removes.Count, pSet.Removes.Count);

            foreach (var add in adds)
            {
                Assert.Contains(add, pSet.Adds);
            }

            foreach (var remove in removes)
            {
                Assert.Contains(remove, pSet.Removes);
            }
        }

        [Theory]
        [AutoData]
        public void Add_AddsElementToAddsSet(TestType value)
        {
            var pSet = new P_Set<TestType>();

            pSet = pSet.Add(value);

            Assert.Contains(value, pSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_Concurrent_AddsOnlyOneElement(TestType value)
        {
            var pSet = new P_Set<TestType>();

            pSet = pSet.Add(value);
            pSet = pSet.Add(value);

            Assert.Equal(1, pSet.Adds.Count(v => Equals(v, value)));
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(TestType value)
        {
            var pSet = new P_Set<TestType>();

            var newPSet = pSet.Remove(value);

            Assert.Same(pSet, newPSet);
        }

        [Theory]
        [AutoData]
        public void Remove_AddsElementToRemovesSet(TestType value)
        {
            var pSet = new P_Set<TestType>();

            pSet = pSet.Add(value);
            pSet = pSet.Remove(value);

            Assert.Contains(value, pSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Remove_Concurrent_AddsOnlyOneElementToRemoveSet(TestType value)
        {
            var pSet = new P_Set<TestType>();

            pSet = pSet.Add(value);
            pSet = pSet.Remove(value);
            pSet = pSet.Remove(value);

            Assert.Equal(1, pSet.Removes.Count(v => Equals(v, value)));
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value)
        {
            var pSet = new P_Set<TestType>();

            pSet = pSet.Add(value);

            var lookup = pSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value)
        {
            var pSet = new P_Set<TestType>();

            pSet = pSet.Add(value);
            pSet = pSet.Remove(value);

            var lookup = pSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsFalse(TestType value)
        {
            var pSet = new P_Set<TestType>();

            pSet = pSet.Add(value);
            pSet = pSet.Remove(value);
            pSet = pSet.Add(value);

            var lookup = pSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesAddsAndRemoves(TestType one, TestType two, TestType three, TestType four, TestType five)
        {
            var pSet = new P_Set<TestType>(new[] { one, two }.ToImmutableHashSet(), new[] { three }.ToImmutableHashSet());

            var newPSet = pSet.Merge(new[] { three, four }.ToImmutableHashSet(), new[] { five }.ToImmutableHashSet());

            Assert.Equal(4, newPSet.Adds.Count);
            Assert.Equal(1, newPSet.Removes.Count);
            Assert.Contains(one, newPSet.Adds);
            Assert.Contains(two, newPSet.Adds);
            Assert.Contains(three, newPSet.Adds);
            Assert.Contains(four, newPSet.Adds);
            Assert.Contains(three, newPSet.Removes);
        }
    }
}
