using System;
using AutoFixture.Xunit2;
using Cluster.Entities;
using LWW_Register.StateBased;
using UnitTestHelpers.TestTypes;
using Xunit;
using static UnitTestHelpers.GuidHelpers;

namespace LWW_Register.UnitTests
{
    public class StateBasedLWWRegisterTests
    {
        [Theory]
        [AutoData]
        public void Merge_LeftClockWithLowerTimestamp_ReturnsRightObject(
            TestType leftValue, TestType rightValue, Node leftNode, Node rightNode)
        {
            var ts = DateTime.Now.Ticks;
            var leftLww = new LWW_Register<TestType>(leftValue, leftNode, ts);
            var rightLww = new LWW_Register<TestType>(rightValue, rightNode, ts + 100);

            var result = leftLww.Merge(rightLww);

            Assert.Same(rightLww, result);
        }

        [Theory]
        [AutoData]
        public void Merge_LeftClockWithHigherTimestamp_ReturnsLeftObject(
            TestType leftValue, TestType rightValue, Node leftNode, Node rightNode)
        {
            var ts = DateTime.Now.Ticks;
            var leftLww = new LWW_Register<TestType>(leftValue, leftNode, ts + 100);
            var rightLww = new LWW_Register<TestType>(rightValue, rightNode, ts);

            var result = leftLww.Merge(rightLww);

            Assert.Same(leftLww, result);
        }

        [Theory]
        [AutoData]
        public void Merge_RightClockWithLowerTimestamp_ReturnsLeftObject(
            TestType leftValue, TestType rightValue, Node leftNode, Node rightNode)
        {
            var ts = DateTime.Now.Ticks;
            var leftLww = new LWW_Register<TestType>(leftValue, leftNode, ts + 100);
            var rightLww = new LWW_Register<TestType>(rightValue, rightNode, ts);

            var result = leftLww.Merge(rightLww);

            Assert.Same(leftLww, result);
        }

        [Theory]
        [AutoData]
        public void Merge_RightClockWithHigherTimestamp_ReturnsRightObject(
            TestType leftValue, TestType rightValue, Node leftNode, Node rightNode)
        {
            var ts = DateTime.Now.Ticks;
            var leftLww = new LWW_Register<TestType>(leftValue, leftNode, ts);
            var rightLww = new LWW_Register<TestType>(rightValue, rightNode, ts + 100);

            var result = leftLww.Merge(rightLww);

            Assert.Same(rightLww, result);
        }

        [Theory]
        [AutoData]
        public void Merge_SameValues_LeftNodeSmallerId_ReturnsLeftObject(
            TestType leftValue, TestType rightValue)
        {
            var ts = DateTime.Now.Ticks;
            var leftLww = new LWW_Register<TestType>(leftValue, new Node(GenerateGuid('a', Guid.Empty)), ts);
            var rightLww = new LWW_Register<TestType>(rightValue, new Node(GenerateGuid('b', Guid.Empty)), ts);

            var result = leftLww.Merge(rightLww);

            Assert.Same(leftLww, result);
        }

        [Theory]
        [AutoData]
        public void Merge_SameValues_RightNodeSmallerId_ReturnsRightObject(
            TestType leftValue, TestType rightValue)
        {
            var ts = DateTime.Now.Ticks;
            var leftLww = new LWW_Register<TestType>(leftValue, new Node(GenerateGuid('b', Guid.Empty)), ts);
            var rightLww = new LWW_Register<TestType>(rightValue, new Node(GenerateGuid('a', Guid.Empty)), ts);

            var result = leftLww.Merge(rightLww);

            Assert.Same(rightLww, result);
        }
    }
}
