using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Sets.Commutative.TwoPhase;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Sets.UnitTests.Commutative
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
        public void Add_AddsElementToAddsSet(TestType value)
        {
            var pSet = new P_OptimizedSet<TestType>();

            pSet = pSet.Add(value);

            var expectedElement = new P_OptimizedSetElement<TestType>(value, false);
            Assert.Contains(expectedElement, pSet.Elements);
        }

        [Theory]
        [AutoData]
        public void Add_Concurrent_AddsOnlyOneElement(TestType value)
        {
            var pSet = new P_OptimizedSet<TestType>();

            pSet = pSet.Add(value);
            pSet = pSet.Add(value);

            Assert.Equal(1, pSet.Elements.Count(v => Equals(v.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(TestType value)
        {
            var pSet = new P_OptimizedSet<TestType>();

            var newPSet = pSet.Remove(value);

            Assert.Same(pSet, newPSet);
        }

        [Theory]
        [AutoData]
        public void Remove_AddsRemovedFlagToElement(TestType value)
        {
            var pSet = new P_OptimizedSet<TestType>();

            pSet = pSet.Add(value);
            pSet = pSet.Remove(value);

            var expectedElement = new P_OptimizedSetElement<TestType>(value, true);
            var notExpectedElement = new P_OptimizedSetElement<TestType>(value, false);

            Assert.Contains(expectedElement, pSet.Elements);
            Assert.DoesNotContain(notExpectedElement, pSet.Elements);
        }

        [Theory]
        [AutoData]
        public void Remove_Concurrent_AddsOnlyOneElementToRemoveSet(TestType value)
        {
            var pSet = new P_OptimizedSet<TestType>();

            pSet = pSet.Add(value);
            pSet = pSet.Remove(value);
            pSet = pSet.Remove(value);

            Assert.Equal(1, pSet.Elements.Count(v => Equals(v.Value, value) && v.Removed));
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value)
        {
            var pSet = new P_OptimizedSet<TestType>();

            pSet = pSet.Add(value);

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
    }
}
