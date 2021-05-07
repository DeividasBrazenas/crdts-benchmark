using System.Collections.Immutable;
using AutoFixture.Xunit2;
using CRDT.Sets.Convergent.LastWriterWins;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Sets.UnitTests.Convergent
{
    public class LWW_OptimizedSetTests
    {
        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value, long timestamp)
        {
            var lwwSet = new LWW_OptimizedSet<TestType>();
            lwwSet = lwwSet.Merge(new[] { new LWW_OptimizedSetElement<TestType>(value, timestamp, false) }.ToImmutableHashSet());

            var lookup = lwwSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value, long timestamp)
        {
            var lwwSet = new LWW_OptimizedSet<TestType>();

            lwwSet.Add(value, timestamp);
            lwwSet.Remove(value, timestamp + 1);

            var lookup = lwwSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsTrue(TestType value, long timestamp)
        {
            var lwwSet = new LWW_OptimizedSet<TestType>();

            var add = new LWW_OptimizedSetElement<TestType>(value, timestamp, false);
            var remove = new LWW_OptimizedSetElement<TestType>(value, timestamp + 10, true);
            var reAdd = new LWW_OptimizedSetElement<TestType>(value, timestamp + 100, false);

            lwwSet = lwwSet.Merge(new[] { new LWW_OptimizedSetElement<TestType>(value, timestamp, false) }.ToImmutableHashSet());
            lwwSet = lwwSet.Merge(new[] { new LWW_OptimizedSetElement<TestType>(value, timestamp + 10, true) }.ToImmutableHashSet());
            lwwSet = lwwSet.Merge(new[] { new LWW_OptimizedSetElement<TestType>(value, timestamp + 100, false) }.ToImmutableHashSet());

            var lookup = lwwSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesAddsAndRemoves(TestType one, TestType two, TestType three, long timestamp)
        {
            var elementOne = new LWW_OptimizedSetElement<TestType>(one, timestamp, false);
            var elementTwo = new LWW_OptimizedSetElement<TestType>(two, timestamp + 1, true);
            var elementThree = new LWW_OptimizedSetElement<TestType>(one, timestamp + 2, true);
            var elementFour = new LWW_OptimizedSetElement<TestType>(three, timestamp + 3, false);
            var elementFive = new LWW_OptimizedSetElement<TestType>(two, timestamp, true);

            var lwwSet = new LWW_OptimizedSet<TestType>(new[] { elementOne, elementTwo }.ToImmutableHashSet());

            var newLwwSet = lwwSet.Merge(new[] { elementThree, elementFour, elementFive }.ToImmutableHashSet());

            Assert.Equal(5, newLwwSet.Elements.Count);
            Assert.Contains(newLwwSet.Elements, e => Equals(e, elementTwo));
            Assert.Contains(newLwwSet.Elements, e => Equals(e, elementThree));
            Assert.Contains(newLwwSet.Elements, e => Equals(e, elementFour));
        }
    }
}
