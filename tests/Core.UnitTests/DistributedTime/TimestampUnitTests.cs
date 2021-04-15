using CRDT.Core.DistributedTime;
using Xunit;

namespace CRDT.Core.UnitTests.DistributedTime
{
    public class TimestampUnitTests
    {
        [Fact]
        public void Compare_BothNull_ReturnsZero()
        {
            Timestamp left = null;
            Timestamp right = null;

            var result = Timestamp.Compare(left, right);

            Assert.Equal(0, result);
        }

        [Fact]
        public void Compare_LeftIsNull_Returns1()
        {
            Timestamp left = null;
            Timestamp right = new Timestamp();

            var result = Timestamp.Compare(left, right);

            Assert.Equal(1, result);
        }

        [Fact]
        public void Compare_RightIsNull_ReturnsMinus1()
        {
            Timestamp left = new Timestamp();
            Timestamp right = null;

            var result = Timestamp.Compare(left, right);

            Assert.Equal(-1, result);
        }

        [Fact]
        public void Compare_LeftIsHigher_ReturnsMinus1()
        {
            Timestamp left = new Timestamp(1000);
            Timestamp right = new Timestamp(100);

            var result = Timestamp.Compare(left, right);

            Assert.Equal(-1, result);
        }

        [Fact]
        public void Compare_RightIsHigher_Returns1()
        {
            Timestamp left = new Timestamp(100);
            Timestamp right = new Timestamp(1000);

            var result = Timestamp.Compare(left, right);

            Assert.Equal(1, result);
        }

        [Fact]
        public void Compare_BothEqual_Returns0()
        {
            Timestamp left = new Timestamp(100);
            Timestamp right = new Timestamp(100);

            var result = Timestamp.Compare(left, right);

            Assert.Equal(0, result);
        }
    }
}