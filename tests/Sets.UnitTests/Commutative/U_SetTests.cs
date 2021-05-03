using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Sets.Commutative;
using CRDT.Sets.Commutative.TwoPhase;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Sets.UnitTests.Commutative
{
    public class U_SetTests
    {
        [Theory]
        [AutoData]
        public void Create_CreatesSetWithElements(U_SetElement<TestType> one, U_SetElement<TestType> two, U_SetElement<TestType> three)
        {
            var elements = new[] { one, two, three }.ToImmutableHashSet();

            var uSet = new U_Set<TestType>(elements);

            Assert.Equal(elements.Count, uSet.Elements.Count);

            foreach (var add in elements)
            {
                Assert.Contains(add, uSet.Elements);
            }
        }

        [Theory]
        [AutoData]
        public void Add_AddsElementToAddsSet(TestType value)
        {
            var uSet = new U_Set<TestType>();

            uSet = uSet.Add(value);

            var expectedElement = new U_SetElement<TestType>(value, false);
            Assert.Contains(expectedElement, uSet.Elements);
        }

        [Theory]
        [AutoData]
        public void Add_Concurrent_AddsOnlyOneElement(TestType value)
        {
            var uSet = new U_Set<TestType>();

            uSet = uSet.Add(value);
            uSet = uSet.Add(value);

            Assert.Equal(1, uSet.Elements.Count(v => Equals(v.Value, value)));
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(TestType value)
        {
            var uSet = new U_Set<TestType>();

            var newPSet = uSet.Remove(value);

            Assert.Same(uSet, newPSet);
        }

        [Theory]
        [AutoData]
        public void Remove_AddsRemovedFlagToElement(TestType value)
        {
            var uSet = new U_Set<TestType>();

            uSet = uSet.Add(value);
            uSet = uSet.Remove(value);

            var expectedElement = new U_SetElement<TestType>(value, true);
            var notExpectedElement = new U_SetElement<TestType>(value, false);

            Assert.Contains(expectedElement, uSet.Elements);
            Assert.DoesNotContain(notExpectedElement, uSet.Elements);
        }

        [Theory]
        [AutoData]
        public void Remove_Concurrent_AddsOnlyOneElementToRemoveSet(TestType value)
        {
            var uSet = new U_Set<TestType>();

            uSet = uSet.Add(value);
            uSet = uSet.Remove(value);
            uSet = uSet.Remove(value);

            Assert.Equal(1, uSet.Elements.Count(v => Equals(v.Value, value) && v.Removed));
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value)
        {
            var uSet = new U_Set<TestType>();

            uSet = uSet.Add(value);

            var lookup = uSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value)
        {
            var uSet = new U_Set<TestType>();

            uSet = uSet.Add(value);
            uSet = uSet.Remove(value);

            var lookup = uSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsFalse(TestType value)
        {
            var uSet = new U_Set<TestType>();

            uSet = uSet.Add(value);
            uSet = uSet.Remove(value);
            uSet = uSet.Add(value);

            var lookup = uSet.Lookup(value);

            Assert.False(lookup);
        }
    }
}
