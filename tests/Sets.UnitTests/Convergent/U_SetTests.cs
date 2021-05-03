using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Sets.Convergent;
using CRDT.Sets.Convergent.TwoPhase;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Sets.UnitTests.Convergent
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
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value)
        {
            var uSet = new U_Set<TestType>();

            uSet = uSet.Merge(new[] { new U_SetElement<TestType>(value, false) }.ToImmutableHashSet());

            var lookup = uSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value)
        {
            var uSet = new U_Set<TestType>();

            uSet = uSet.Merge(new[] { new U_SetElement<TestType>(value, false) }.ToImmutableHashSet());
            uSet = uSet.Merge(new[] { new U_SetElement<TestType>(value, true) }.ToImmutableHashSet());

            var lookup = uSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_ReAdded_ReturnsFalse(TestType value)
        {
            var uSet = new U_Set<TestType>();

            uSet = uSet.Merge(new[] { new U_SetElement<TestType>(value, false) }.ToImmutableHashSet());
            uSet = uSet.Merge(new[] { new U_SetElement<TestType>(value, true) }.ToImmutableHashSet());
            uSet = uSet.Merge(new[] { new U_SetElement<TestType>(value, false) }.ToImmutableHashSet());

            var lookup = uSet.Lookup(value);

            Assert.False(lookup);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesAddsAndRemoves(TestType one, TestType two, TestType three)
        {
            var elementOne = new U_SetElement<TestType>(one, false);
            var elementTwo = new U_SetElement<TestType>(two, true);
            var elementThree = new U_SetElement<TestType>(one, true);
            var elementFour = new U_SetElement<TestType>(three, false);

            var uSet = new U_Set<TestType>(new[] { elementOne, elementTwo }.ToImmutableHashSet());

            var newUSet = uSet.Merge(new[] { elementThree, elementFour }.ToImmutableHashSet());

            Assert.Equal(3, newUSet.Elements.Count);
            Assert.Contains(newUSet.Elements, e => Equals(e, elementTwo));
            Assert.Contains(newUSet.Elements, e => Equals(e, elementThree));
            Assert.Contains(newUSet.Elements, e => Equals(e, elementFour));
        }
    }
}
