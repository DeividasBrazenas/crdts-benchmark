using AutoFixture.Xunit2;
using CRDT.Registers.Convergent;
using CRDT.Registers.Convergent.LastWriterWins;
using CRDT.Registers.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Registers.UnitTests.Convergent
{
    public class LWW_RegisterTests
    {
        [Theory]
        [AutoData]
        public void Merge_LeftClockWithHigherTimestamp_ReturnsLeftObject(
            TestType leftValue, TestType rightValue)
        {
            var lww = new LWW_Register<TestType>(new LWW_RegisterElement<TestType>(leftValue, 1));
            var result = lww.Assign(rightValue, 0);

            Assert.Same(lww, result);
            Assert.Same(leftValue, result.Element.Value);
        }

        [Theory]
        [AutoData]
        public void Merge_RightClockWithHigherTimestamp_ReturnsRightObject(
            TestType leftValue, TestType rightValue)
        {
            var lww = new LWW_Register<TestType>(new LWW_RegisterElement<TestType>(leftValue, 0));
            var result = lww.Assign(rightValue, 1);

            Assert.Same(rightValue, result.Element.Value);
        }
    }
}