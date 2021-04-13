using System.Collections.Immutable;
using AutoFixture.Xunit2;
using CRDT.Sets.Convergent;
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
        public void Add_AddsElementToTheSet(TestType[] existingElements, TestType value)
        {
            var gSet = new G_Set<TestType>(existingElements.ToImmutableHashSet());

            gSet = gSet.Add(value);

            Assert.Contains(value, gSet.Values);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesValues(TestType one, TestType two, TestType three)
        {
            var firstGSet = new G_Set<TestType>(new[] { one, two }.ToImmutableHashSet());

            var secondGSet = new G_Set<TestType>(new[] { two, three }.ToImmutableHashSet());

            var gSet = firstGSet.Merge(secondGSet);

            Assert.Equal(3, gSet.Values.Count);
            Assert.Contains(one, gSet.Values);
            Assert.Contains(two, gSet.Values);
            Assert.Contains(three, gSet.Values);
        }
    }
}
