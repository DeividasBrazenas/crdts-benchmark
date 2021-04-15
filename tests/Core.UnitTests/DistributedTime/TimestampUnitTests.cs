using CRDT.Core.DistributedTime;
using Xunit;

namespace CRDT.Core.UnitTests.DistributedTime
{
    public class TimestampUnitTests
    {
        [Fact]
        public void Compare_RightIsNull_Returns()
        {
            Timestamp left = new Timestamp();
            Timestamp right = null;

            var result = left.CompareTo(right);

            Assert.Equal(1, result);
        }

        [Fact]
        public void Compare_LeftIsHigher_Returns1()
        {
            Timestamp left = new Timestamp(1000);
            Timestamp right = new Timestamp(100);

            var result = left.CompareTo(right);

            Assert.Equal(1, result);
        }

        [Fact]
        public void Compare_RightIsHigher_ReturnsMinus1()
        {
            Timestamp left = new Timestamp(100);
            Timestamp right = new Timestamp(1000);

            var result = left.CompareTo(right);

            Assert.Equal(-1, result);
        }

        [Fact]
        public void Compare_BothEqual_Returns0()
        {
            Timestamp left = new Timestamp(100);
            Timestamp right = new Timestamp(100);

            var result = left.CompareTo(right);

            Assert.Equal(0, result);
        }
    }
}