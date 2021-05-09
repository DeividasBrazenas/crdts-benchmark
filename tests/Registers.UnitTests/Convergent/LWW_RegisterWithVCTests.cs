using System.Collections.Immutable;
using AutoFixture.Xunit2;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;
using CRDT.Registers.Convergent.LastWriterWins;
using CRDT.Registers.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Registers.UnitTests.Convergent
{
    public class LWW_RegisterWithVCTests
    {
        [Theory]
        [AutoData]
        public void Merge_LeftClockWithHigherTimestamp_ReturnsLeftObject(
            TestType leftValue, TestType rightValue, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var leftClock = new VectorClock(clock.Add(node, 1));
            var rightClock = new VectorClock(clock.Add(node, 0));

            var lww = new LWW_RegisterWithVC<TestType>(new LWW_RegisterWithVCElement<TestType>(leftValue, leftClock, false));
            var result = lww.Assign(rightValue, rightClock);

            Assert.Same(lww, result);
            Assert.Same(leftValue, result.Element.Value);
        }

        [Theory]
        [AutoData]
        public void Merge_RightClockWithHigherTimestamp_ReturnsRightObject(
            TestType leftValue, TestType rightValue, Node node)
        {
            var clock = ImmutableSortedDictionary<Node, long>.Empty;

            var leftClock = new VectorClock(clock.Add(node, 1));
            var rightClock = new VectorClock(clock.Add(node, 2));

            var lww = new LWW_RegisterWithVC<TestType>(new LWW_RegisterWithVCElement<TestType>(leftValue, leftClock, false));
            var result = lww.Assign(rightValue, rightClock);

            Assert.Equal(rightValue, result.Element.Value);
        }
    }
}