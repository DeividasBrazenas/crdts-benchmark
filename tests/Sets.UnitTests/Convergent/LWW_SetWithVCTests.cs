using System.Collections.Immutable;
using AutoFixture.Xunit2;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using CRDT.Sets.Convergent;
using CRDT.Sets.Convergent.LastWriterWins;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Sets.UnitTests.Convergent
{
    public class LWW_SetWithVCTests
    {
        [Theory]
        [AutoData]
        public void Create_CreatesSetWithElements(LWW_SetWithVCElement<TestType> one, LWW_SetWithVCElement<TestType> two, LWW_SetWithVCElement<TestType> three)
        {
            var adds = new[] { one, two }.ToImmutableHashSet();
            var removes = new[] { two, three }.ToImmutableHashSet();

            var lwwSet = new LWW_SetWithVC<TestType>(adds, removes);

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
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_SetWithVC<TestType>();

            var element = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)));

            lwwSet = lwwSet.Merge(new[] { element }.ToImmutableHashSet(), ImmutableHashSet<LWW_SetWithVCElement<TestType>>.Empty);

            var lookup = lwwSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_SetWithVC<TestType>();

            var add = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)));
            var remove = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)));

            lwwSet = lwwSet.Merge(new[] { add }.ToImmutableHashSet(), new[] { remove }.ToImmutableHashSet());

            var lookup = lwwSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsTrue(TestType value, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;
            var lwwSet = new LWW_SetWithVC<TestType>();

            var add = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 0)));
            var remove = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 1)));
            var reAdd = new LWW_SetWithVCElement<TestType>(value, new VectorClock(clock.Add(node, 2)));

            lwwSet = lwwSet.Merge(new[] { add, reAdd }.ToImmutableHashSet(), new[] { remove }.ToImmutableHashSet());

            var lookup = lwwSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesAddsAndRemoves(LWW_SetWithVCElement<TestType> one, LWW_SetWithVCElement<TestType> two, LWW_SetWithVCElement<TestType> three, LWW_SetWithVCElement<TestType> four, LWW_SetWithVCElement<TestType> five)
        {
            var lwwSet = new LWW_SetWithVC<TestType>(new[] { one, two }.ToImmutableHashSet(), new[] { three }.ToImmutableHashSet());

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
