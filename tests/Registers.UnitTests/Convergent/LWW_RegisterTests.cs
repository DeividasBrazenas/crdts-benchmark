using System;
using AutoFixture.Xunit2;
using CRDT.Core.Cluster;
using CRDT.Registers.Convergent;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;
using static CRDT.UnitTestHelpers.GuidHelpers;

namespace CRDT.Registers.UnitTests.Convergent
{
    public class LWW_RegisterTests
    {
        [Theory]
        [AutoData]
        public void Merge_LeftClockWithHigherTimestamp_ReturnsLeftObject(
            TestType leftValue, TestType rightValue, Node leftNode, Node rightNode)
        {
            var lww = new LWW_Register<TestType>(leftValue, leftNode, 1);
            var result = lww.Merge(rightValue, rightNode, 0);

            Assert.Same(lww, result);
            Assert.Same(leftValue, result.Value);
            Assert.Equal(leftNode, result.UpdatedBy);
        }

        [Theory]
        [AutoData]
        public void Merge_RightClockWithHigherTimestamp_ReturnsRightObject(
            TestType leftValue, TestType rightValue, Node leftNode, Node rightNode)
        {
            var lww = new LWW_Register<TestType>(leftValue, leftNode, 0);
            var result = lww.Merge(rightValue, rightNode, 1);

            Assert.Same(rightValue, result.Value);
            Assert.Equal(rightNode, result.UpdatedBy);
        }

        [Theory]
        [AutoData]
        public void Merge_SameValues_LeftNodeSmallerId_ReturnsLeftObject(TestType leftValue, TestType rightValue)
        {
            var ts = DateTime.Now.Ticks;
            var lww = new LWW_Register<TestType>(leftValue, new Node(GenerateGuid('a', Guid.Empty)), ts);
            var result = lww.Merge(rightValue, new Node(GenerateGuid('b', Guid.Empty)), ts);

            Assert.Same(lww, result);
            Assert.Same(leftValue, result.Value);
        }

        [Theory]
        [AutoData]
        public void Merge_SameValues_RightNodeSmallerId_ReturnsRightObject(TestType leftValue, TestType rightValue)
        {
            var ts = DateTime.Now.Ticks;
            var lww = new LWW_Register<TestType>(leftValue, new Node(GenerateGuid('b', Guid.Empty)), ts);
          
            var result = lww.Merge(rightValue, new Node(GenerateGuid('a', Guid.Empty)), ts);
            Assert.Same(rightValue, result.Value);
        }
    }
}