using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Sets.Commutative;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Sets.UnitTests.Commutative
{
    public class P_SetTests
    {
        [Theory]
        [AutoData]
        public void Create_CreatesSetWithElements(TestType one, TestType two, TestType three)
        {
            var adds = new[] { one, two }.ToImmutableHashSet();
            var removes = new[] { two, three }.ToImmutableHashSet();

            var pSet = new P_Set<TestType>(adds, removes);

            Assert.Equal(adds.Count, pSet.Adds.Count);
            Assert.Equal(removes.Count, pSet.Removes.Count);

            foreach (var add in adds)
            {
                Assert.Contains(add, pSet.Adds);
            }

            foreach (var remove in removes)
            {
                Assert.Contains(remove, pSet.Removes);
            }
        }

        [Theory]
        [AutoData]
        public void Add_AddsElementToAddsSet(TestType value)
        {
            var pSet = new P_Set<TestType>();

            pSet = pSet.Add(value);

            Assert.Contains(value, pSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_Concurrent_AddsOnlyOneElement(TestType value)
        {
            var pSet = new P_Set<TestType>();

            pSet = pSet.Add(value);
            pSet = pSet.Add(value);

            Assert.Equal(1, pSet.Adds.Count(v => Equals(v, value)));
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(TestType value)
        {
            var pSet = new P_Set<TestType>();

            var newPSet = pSet.Remove(value);

            Assert.Equal(pSet, newPSet);
        }

        [Theory]
        [AutoData]
        public void Remove_AddsElementToRemovesSet(TestType value)
        {
            var pSet = new P_Set<TestType>();

            pSet = pSet.Add(value);
            pSet = pSet.Remove(value);

            Assert.Contains(value, pSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Remove_Concurrent_AddsOnlyOneElementToRemoveSet(TestType value)
        {
            var pSet = new P_Set<TestType>();

            pSet = pSet.Add(value);
            pSet = pSet.Remove(value);
            pSet = pSet.Remove(value);

            Assert.Equal(1, pSet.Removes.Count(v => Equals(v, value)));
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndNotRemoved_ReturnsTrue(TestType value)
        {
            var pSet = new P_Set<TestType>();

            pSet = pSet.Add(value);

            var lookup = pSet.Lookup(value);

            Assert.True(lookup);
        }

        [Theory]
        [AutoData]
        public void Lookup_AddedAndRemoved_ReturnsFalse(TestType value)
        {
            var pSet = new P_Set<TestType>();

            pSet = pSet.Add(value);
            pSet = pSet.Remove(value);

            var lookup = pSet.Lookup(value);

            Assert.False(lookup);
        }
    }
}
