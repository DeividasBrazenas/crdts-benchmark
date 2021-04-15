using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Sets.Commutative;
using CRDT.UnitTestHelpers.TestTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public void Add_AddsElementToTheSet(TestType[] existingElements, TestType value)
        {
            var gSet = new G_Set<TestType>(existingElements.ToImmutableHashSet());

            var valueJson = JsonConvert.SerializeObject(value);

            gSet = gSet.Add(new Operation(JToken.Parse(valueJson)));

            Assert.Contains(value, gSet.Values);
        }

        [Theory]
        [AutoData]
        public void Add_Concurrent_AddsOnlyOneElement(TestType[] existingElements, TestType value)
        {
            var gSet = new G_Set<TestType>(existingElements.ToImmutableHashSet());

            var valueJson = JsonConvert.SerializeObject(value);

            gSet = gSet.Add(new Operation(JToken.Parse(valueJson)));
            gSet = gSet.Add(new Operation(JToken.Parse(valueJson)));

            Assert.Equal(1, gSet.Values.Count(v => Equals(v, value)));
        }

        [Theory]
        [AutoData]
        public void Merge_MergesValues(TestType one, TestType two, TestType three)
        {
            var firstGSet = new G_Set<TestType>(new[] { one, two }.ToImmutableHashSet());

            var secondGSet = new G_Set<TestType>(new[] { two, three }.ToImmutableHashSet());

            var newGSet = firstGSet.Merge(secondGSet);

            Assert.Equal(3, newGSet.Values.Count);
            Assert.Contains(one, newGSet.Values);
            Assert.Contains(two, newGSet.Values);
            Assert.Contains(three, newGSet.Values);
        }
    }
}
