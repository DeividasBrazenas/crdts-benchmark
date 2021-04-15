using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using Xunit;
using static CRDT.UnitTestHelpers.GuidHelpers;

namespace CRDT.Core.UnitTests.DistributedTime
{
    public class VectorClockUnitTests
    {
        [Theory]
        [AutoData]
        public void Equals_ReturnsTrue(Node node, long value)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (node, value),
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (node, value),
            });

            var result = leftClock.Equals(rightClock);

            Assert.True(result);
        }

        [Theory]
        [AutoData]
        public void Equals_ReturnsFalse(Node node, long value)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (node, value),
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (node, value + 1),
            });

            var result = leftClock.Equals(rightClock);

            Assert.False(result);
        }

        [Theory]
        [AutoData]
        public void Increment_ExistingNode_ReturnsIncrementedVectorClock(Node node1, Node node2, long value1, long value2)
        {
            var clock = BuildClock(new List<(Node, long)>
            {
                (node1, value1),
                (node2, value2),
            });

            var newClock = clock.Increment(node2);

            Assert.Equal(value1, newClock.Values.GetValueOrDefault(node1));
            Assert.Equal(value2 + 1, newClock.Values.GetValueOrDefault(node2));
        }

        [Theory]
        [AutoData]
        public void Increment_NonExistingNode_ReturnsSameVectorClock(Node node1, Node node2, long value1)
        {
            var clock = BuildClock(new List<(Node, long)>
            {
                (node1, value1),
            });

            var newClock = clock.Increment(node2);

            Assert.Same(newClock, clock);
        }

        [Theory]
        [AutoData]
        public void Compare_SameValues_ReturnsOrderSame(Guid id1, Guid id2, Guid id3)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 182),
                (new Node(id2), 193),
                (new Node(id3), 164)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 182),
                (new Node(id2), 193),
                (new Node(id3), 164)
            });

            var isSame = leftClock.IsSameAs(rightClock);

            Assert.True(isSame);
        }

        [Theory]
        [AutoData]
        public void Compare_RightClockLowerValues_ReturnsOrderAfter(Guid id1, Guid id2, Guid id3)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 182),
                (new Node(id2), 193),
                (new Node(id3), 164)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 180),
                (new Node(id2), 190),
                (new Node(id3), 160)
            });

            var isAfter = leftClock.IsAfter(rightClock);

            Assert.True(isAfter);
        }

        [Theory]
        [AutoData]
        public void Compare_RightClockOrderBeforeToConcurrent_ReturnsOrderConcurrent(Guid id1, Guid id2, Guid id3)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(GenerateGuid('a', id1)), 182),
                (new Node(GenerateGuid('b', id2)), 193),
                (new Node(GenerateGuid('c', id3)), 164)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(GenerateGuid('a', id1)), 180),
                (new Node(GenerateGuid('b', id2)), 195),
                (new Node(GenerateGuid('c', id3)), 160)
            });

            var isConcurrent = leftClock.IsConcurrentWith(rightClock);

            Assert.True(isConcurrent);
        }

        [Theory]
        [AutoData]
        public void Compare_RightClockOrderAfterToConcurrent_ReturnsOrderConcurrent(Guid id1, Guid id2, Guid id3)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(GenerateGuid('a', id1)), 182),
                (new Node(GenerateGuid('b', id2)), 193),
                (new Node(GenerateGuid('c', id3)), 164)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(GenerateGuid('a', id1)), 190),
                (new Node(GenerateGuid('b', id2)), 178),
                (new Node(GenerateGuid('c', id3)), 170)
            });

            var isConcurrent = leftClock.IsConcurrentWith(rightClock);

            Assert.True(isConcurrent);
        }

        [Theory]
        [AutoData]
        public void Compare_RightClockHigherValues_ReturnsOrderBefore(Guid id1, Guid id2, Guid id3)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 182),
                (new Node(id2), 193),
                (new Node(id3), 164)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 197),
                (new Node(id2), 242),
                (new Node(id3), 198)
            });

            var isBefore = leftClock.IsBefore(rightClock);

            Assert.True(isBefore);
        }

        [Theory]
        [AutoData]
        public void Compare_MixedValues_ReturnsOrderConcurrent(Guid id1, Guid id2, Guid id3)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 182),
                (new Node(id2), 193),
                (new Node(id3), 164)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 197),
                (new Node(id2), 170),
                (new Node(id3), 198)
            });

            var isConcurrent = leftClock.IsConcurrentWith(rightClock);

            Assert.True(isConcurrent);
        }

        [Theory]
        [AutoData]
        public void Compare_LessNodesWithLowerValuesOnLeftClock_ReturnsOrderBefore(Guid id1, Guid id2, Guid id3)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 182),
                (new Node(id2), 193)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 190),
                (new Node(id2), 199),
                (new Node(id3), 199)
            });

            var isBefore = leftClock.IsBefore(rightClock);

            Assert.True(isBefore);
        }

        [Theory]
        [AutoData]
        public void Compare_LessNodesWithHigherValuesOnLeftClock_ReturnsOrderConcurrent(Guid id1, Guid id2, Guid id3)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 205),
                (new Node(id2), 208)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 190),
                (new Node(id2), 199),
                (new Node(id3), 199)
            });

            var isConcurrent = leftClock.IsConcurrentWith(rightClock);

            Assert.True(isConcurrent);
        }

        [Theory]
        [AutoData]
        public void Compare_LessNodesWithSameValuesOnLeftClock_ReturnsOrderBefore(Guid id1, Guid id2, Guid id3)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 205),
                (new Node(id2), 208)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 205),
                (new Node(id2), 208),
                (new Node(id3), 199)
            });

            var isBefore = leftClock.IsBefore(rightClock);

            Assert.True(isBefore);
        }

        [Theory]
        [AutoData]
        public void Compare_MoreNodesWithMixedValuesOnLeftClock_ReturnsOrderConcurrent(Guid id1, Guid id2, Guid id3)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 182),
                (new Node(id2), 209),
                (new Node(id3), 193)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 203),
                (new Node(id2), 207)
            });

            var isConcurrent = leftClock.IsConcurrentWith(rightClock);

            Assert.True(isConcurrent);
        }

        [Theory]
        [AutoData]
        public void Compare_LessNodesWithLowerValuesOnRightClock_ReturnsOrderAfter(Guid id1, Guid id2, Guid id3)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 182),
                (new Node(id2), 193),
                (new Node(id3), 193)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 170),
                (new Node(id2), 183)
            });

            var isAfter = leftClock.IsAfter(rightClock);

            Assert.True(isAfter);
        }

        [Theory]
        [AutoData]
        public void Compare_LessNodesWithHigherValuesOnRightClock_ReturnsOrderConcurrent(Guid id1, Guid id2, Guid id3)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 182),
                (new Node(id2), 193),
                (new Node(id3), 193)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 203),
                (new Node(id2), 207)
            });

            var isConcurrent = leftClock.IsConcurrentWith(rightClock);

            Assert.True(isConcurrent);
        }

        [Theory]
        [AutoData]
        public void Compare_LessNodesWithSameValuesOnRightClock_ReturnsOrderAfter(Guid id1, Guid id2, Guid id3)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 182),
                (new Node(id2), 193),
                (new Node(id3), 193)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 182),
                (new Node(id2), 193)
            });

            var isAfter = leftClock.IsAfter(rightClock);

            Assert.True(isAfter);
        }

        [Theory]
        [AutoData]
        public void Compare_MoreNodesWithMixedValuesOnRightClock_ReturnsOrderConcurrent(Guid id1, Guid id2, Guid id3)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 182),
                (new Node(id2), 209)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 203),
                (new Node(id2), 207),
                (new Node(id3), 193)
            });

            var isConcurrent = leftClock.IsConcurrentWith(rightClock);

            Assert.True(isConcurrent);
        }

        [Theory]
        [AutoData]
        public void Compare_AdditionalNodesInBothClocks_ReturnsOrderConcurrent(Guid id1, Guid id2)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 182),
                (new Node(id2), 209),
                (new Node(Guid.NewGuid()), 209)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(id1), 203),
                (new Node(id2), 207),
                (new Node(Guid.NewGuid()), 193)
            });

            var isConcurrent = leftClock.IsConcurrentWith(rightClock);

            Assert.True(isConcurrent);
        }

        [Theory]
        [AutoData]
        public void Merge_BothClocksWithSameValues_ReturnsSameClock(Guid id1, Guid id2)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(GenerateGuid('a', id1)), 182),
                (new Node(GenerateGuid('b', id2)), 209)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(GenerateGuid('a', id1)), 182),
                (new Node(GenerateGuid('b', id2)), 209)
            });

            var mergedClock = leftClock.Merge(rightClock);

            Assert.Equal(2, mergedClock.Values.Count);
            Assert.Equal(182, mergedClock.Values.First().Value);
            Assert.Equal(209, mergedClock.Values.Skip(1).First().Value);
        }

        [Theory]
        [AutoData]
        public void Merge_BothClocksWithDifferentValues_ReturnsCorrectClock(Guid id1, Guid id2)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(GenerateGuid('a', id1)), 182),
                (new Node(GenerateGuid('b', id2)), 209)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(GenerateGuid('a', id1)), 211),
                (new Node(GenerateGuid('b', id2)), 187)
            });

            var mergedClock = leftClock.Merge(rightClock);

            Assert.Equal(2, mergedClock.Values.Count);
            Assert.Equal(211, mergedClock.Values.First().Value);
            Assert.Equal(209, mergedClock.Values.Skip(1).First().Value);
        }

        [Theory]
        [AutoData]
        public void Merge_LeftClocksWithAdditionalValues_ReturnsCorrectClock(Guid id1, Guid id2)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(GenerateGuid('a', id1)), 182),
                (new Node(GenerateGuid('b', id2)), 209),
                (new Node(GenerateGuid('c', Guid.NewGuid())), 177)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(GenerateGuid('a', id1)), 211),
                (new Node(GenerateGuid('b', id2)), 187)
            });

            var mergedClock = leftClock.Merge(rightClock);

            Assert.Equal(3, mergedClock.Values.Count);
            Assert.Equal(211, mergedClock.Values.First().Value);
            Assert.Equal(209, mergedClock.Values.Skip(1).First().Value);
            Assert.Equal(177, mergedClock.Values.Skip(2).First().Value);
        }

        [Theory]
        [AutoData]
        public void Merge_RightClocksWithAdditionalValues_ReturnsCorrectClock(Guid id1, Guid id2)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(GenerateGuid('a', id1)), 182),
                (new Node(GenerateGuid('b', id2)), 209)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(GenerateGuid('a', id1)), 211),
                (new Node(GenerateGuid('b', id2)), 187),
                (new Node(GenerateGuid('c', Guid.NewGuid())), 177)
            });

            var mergedClock = leftClock.Merge(rightClock);

            Assert.Equal(3, mergedClock.Values.Count);
            Assert.Equal(211, mergedClock.Values.First().Value);
            Assert.Equal(209, mergedClock.Values.Skip(1).First().Value);
            Assert.Equal(177, mergedClock.Values.Skip(2).First().Value);
        }

        [Theory]
        [AutoData]
        public void Merge_BothClocksWithAdditionalValues_ReturnsCorrectClock(Guid id1, Guid id2)
        {
            var leftClock = BuildClock(new List<(Node, long)>
            {
                (new Node(GenerateGuid('a', id1)), 182),
                (new Node(GenerateGuid('b', id2)), 209),
                (new Node(GenerateGuid('c', Guid.NewGuid())), 194),
                (new Node(GenerateGuid('d', Guid.NewGuid())), 206)
            });

            var rightClock = BuildClock(new List<(Node, long)>
            {
                (new Node(GenerateGuid('a', id1)), 211),
                (new Node(GenerateGuid('b', id2)), 197),
                (new Node(GenerateGuid('e', Guid.NewGuid())), 177)
            });

            var mergedClock = leftClock.Merge(rightClock);

            Assert.Equal(5, mergedClock.Values.Count);
            Assert.Equal(211, mergedClock.Values.First().Value);
            Assert.Equal(209, mergedClock.Values.Skip(1).First().Value);
            Assert.Equal(194, mergedClock.Values.Skip(2).First().Value);
            Assert.Equal(206, mergedClock.Values.Skip(3).First().Value);
            Assert.Equal(177, mergedClock.Values.Skip(4).First().Value);
        }

        [Theory]
        [AutoData]
        public void Prune_ReturnsPrunedClock(Node node1, Node node2, Node node3)
        {
            var clock = BuildClock(new List<(Node, long)>
            {
                (node1, 182),
                (node2, 209),
                (node3, 194)
            });

            var prunedClock = clock.Prune(node2);

            Assert.Equal(2, prunedClock.Values.Count);
            Assert.DoesNotContain(prunedClock.Values, x => x.Key == node2);
        }

        private VectorClock BuildClock(IEnumerable<(Node, long)> values)
        {
            var seed = ImmutableSortedDictionary<Node, long>.Empty;

            foreach (var (node, value) in values)
            {
                seed = seed.Add(node, value);
            }

            return new VectorClock(seed);
        }
    }
}
