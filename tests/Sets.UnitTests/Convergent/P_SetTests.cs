using System.Collections.Immutable;
using AutoFixture.Xunit2;
using CRDT.Sets.Convergent.TwoPhase;
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
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value)
        {
            var pSet = new P_Set<TestType>();

            pSet = pSet.Merge(new[] { value }.ToImmutableHashSet(), ImmutableHashSet<TestType>.Empty);

            var lookup = pSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value)
        {
            var pSet = new P_Set<TestType>();

            pSet = pSet.Merge(new[] { value }.ToImmutableHashSet(), ImmutableHashSet<TestType>.Empty);
            pSet = pSet.Merge(ImmutableHashSet<TestType>.Empty, new[] { value }.ToImmutableHashSet());

            var lookup = pSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsFalse(TestType value)
        {
            var pSet = new P_Set<TestType>();

            pSet = pSet.Merge(new[] { value }.ToImmutableHashSet(), ImmutableHashSet<TestType>.Empty);
            pSet = pSet.Merge(ImmutableHashSet<TestType>.Empty, new[] { value }.ToImmutableHashSet());
            pSet = pSet.Merge(new[] { value }.ToImmutableHashSet(), ImmutableHashSet<TestType>.Empty);

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
            Assert.Equal(2, newPSet.Removes.Count);
            Assert.Contains(one, newPSet.Adds);
            Assert.Contains(two, newPSet.Adds);
            Assert.Contains(three, newPSet.Adds);
            Assert.Contains(four, newPSet.Adds);
            Assert.Contains(three, newPSet.Removes);
            Assert.Contains(five, newPSet.Removes);
        }
    }
}
