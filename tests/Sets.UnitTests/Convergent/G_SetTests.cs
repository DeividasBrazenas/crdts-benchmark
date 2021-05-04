using System.Collections.Generic;
using System.Collections.Immutable;
using AutoFixture.Xunit2;
using CRDT.Sets.Convergent.GrowOnly;
using CRDT.UnitTestHelpers.TestTypes;
using Xunit;

namespace CRDT.Sets.UnitTests.Convergent
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
        public void Lookup_ValueDoesNotExist_ReturnsFalse(List<TestType> values, TestType value)
        {
            var gSet = new G_Set<TestType>(values.ToImmutableHashSet());

            var exists = gSet.Lookup(value);

            Assert.False(exists);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesValues(TestType one, TestType two, TestType three)
        {
            var gSet = new G_Set<TestType>(new[] { one, two }.ToImmutableHashSet());

            var values = new[] { two, three }.ToImmutableHashSet();

            var newGSet = gSet.Merge(values);

            Assert.Equal(3, newGSet.Values.Count);
            Assert.Contains(one, newGSet.Values);
            Assert.Contains(two, newGSet.Values);
            Assert.Contains(three, newGSet.Values);
        } 
        
        [Theory]
        [AutoData]
        public void Merge_SameValuesMultipleTimes_MergesOnlyOnce(TestType one, TestType two, TestType three)
        {
            var gSet = new G_Set<TestType>(new[] { one, two }.ToImmutableHashSet());

            var values = new[] { two, three }.ToImmutableHashSet();

            var newGSet = gSet.Merge(values);
            newGSet = gSet.Merge(values);
            newGSet = gSet.Merge(values);

            Assert.Equal(3, newGSet.Values.Count);
            Assert.Contains(one, newGSet.Values);
            Assert.Contains(two, newGSet.Values);
            Assert.Contains(three, newGSet.Values);
        }
    }
}
