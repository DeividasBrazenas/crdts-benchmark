using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Sets.Commutative;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Sets.UnitTests.Commutative
{
    public class G_SetTests
    {
        [Theory]
        [AutoData]
        public void Create_CreatesSetWithElements(TestType one, TestType two, TestType three)
        {
            var values = new[] { one, two, three }.ToImmutableHashSet();

            var gSet = new G_Set<TestType>(values);

            Assert.Equal(values.Count, gSet.Values.Count);

            foreach (var value in values)
            {
                Assert.Contains(value, gSet.Values);
            }
        }

        [Theory]
        [AutoData]
        public void Add_AddsElementToTheSet(TestType value)
        {
            var gSet = new G_Set<TestType>();

            gSet = gSet.Add(value);

            Assert.Contains(value, gSet.Values);
        }

        [Theory]
        [AutoData]
        public void Add_Concurrent_AddsOnlyOneElement(TestType value)
        {
            var gSet = new G_Set<TestType>();

            gSet = gSet.Add(value);
            gSet = gSet.Add(value);

            Assert.Equal(1, gSet.Values.Count(v => Equals(v, value)));
        }

        [Theory]
        [AutoData]
        public void Lookup_ValueExists_ReturnsTrue(List<TestType> values, TestType value)
        {
            var gSet = new G_Set<TestType>(values.ToImmutableHashSet());

            gSet = gSet.Add(value);

            var exists = gSet.Lookup(value);

            Assert.True(exists);
        }

        [Theory]
        [AutoData]
        public void Lookup_ValueDoesNotExist_ReturnsFalse(List<TestType> values, TestType value)
        {
            var gSet = new G_Set<TestType>(values.ToImmutableHashSet());

            var exists = gSet.Lookup(value);

            Assert.False(exists);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesValue(TestType one, TestType two, TestType three)
        {
            var gSet = new G_Set<TestType>(new[] { one, two }.ToImmutableHashSet());

            var newGSet = gSet.Merge(three);

            Assert.Equal(3, newGSet.Values.Count);
            Assert.Contains(one, newGSet.Values);
            Assert.Contains(two, newGSet.Values);
            Assert.Contains(three, newGSet.Values);
        }

        [Theory]
        [AutoData]
        public void Merge_SameValueMultipleTimes_MergesOnlyOnce(TestType one, TestType two, TestType three)
        {
            var gSet = new G_Set<TestType>(new[] { one, two }.ToImmutableHashSet());

            var newGSet = gSet.Merge(three);
            newGSet = gSet.Merge(three);
            newGSet = gSet.Merge(three);
            newGSet = gSet.Merge(three);

            Assert.Equal(3, newGSet.Values.Count);
            Assert.Contains(one, newGSet.Values);
            Assert.Contains(two, newGSet.Values);
            Assert.Contains(three, newGSet.Values);
        }
    }
}
