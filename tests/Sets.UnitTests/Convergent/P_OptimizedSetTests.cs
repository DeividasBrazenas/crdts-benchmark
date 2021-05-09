using System.Collections.Immutable;
using AutoFixture.Xunit2;
using CRDT.Sets.Convergent.TwoPhase;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Sets.UnitTests.Convergent
{
    public class P_OptimizedSetTests
    {
        [Theory]
        [AutoData]
        public void Create_CreatesSetWithElements(P_OptimizedSetElement<TestType> one, P_OptimizedSetElement<TestType> two, P_OptimizedSetElement<TestType> three)
        {
            var elements = new[] { one, two, three }.ToImmutableHashSet();

            var pSet = new P_OptimizedSet<TestType>(elements);

            Assert.Equal(elements.Count, pSet.Elements.Count);

            foreach (var add in elements)
            {
                Assert.Contains(add, pSet.Elements);
            }
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value)
        {
            var pSet = new P_OptimizedSet<TestType>();

            pSet = pSet.Merge(new[] { new P_OptimizedSetElement<TestType>(value, false) }.ToImmutableHashSet());

            var lookup = pSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value)
        {
            var pSet = new P_OptimizedSet<TestType>();

            pSet = pSet.Add(value);
            pSet = pSet.Remove(value);

            var lookup = pSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsFalse(TestType value)
        {
            var pSet = new P_OptimizedSet<TestType>();

            pSet = pSet.Add(value);
            pSet = pSet.Remove(value);
            pSet = pSet.Add(value);

            var lookup = pSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesAddsAndRemoves(TestType one, TestType two, TestType three)
        {
            var elementOne = new P_OptimizedSetElement<TestType>(one, false);
            var elementTwo = new P_OptimizedSetElement<TestType>(two, true);
            var elementThree = new P_OptimizedSetElement<TestType>(one, true);
            var elementFour = new P_OptimizedSetElement<TestType>(three, false);

            var pSet = new P_OptimizedSet<TestType>(new[] { elementOne, elementTwo }.ToImmutableHashSet());

            var newPSet = pSet.Merge(new[] { elementThree, elementFour }.ToImmutableHashSet());

            Assert.Equal(4, newPSet.Elements.Count);
            Assert.Contains(newPSet.Elements, e => Equals(e, elementOne));
            Assert.Contains(newPSet.Elements, e => Equals(e, elementTwo));
            Assert.Contains(newPSet.Elements, e => Equals(e, elementThree));
            Assert.Contains(newPSet.Elements, e => Equals(e, elementFour));
        }
    }
}
