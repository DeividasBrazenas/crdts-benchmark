using System.Collections.Immutable;
using System.Linq;
using AutoFixture.Xunit2;
using CRDT.Sets.Commutative;
using CRDT.Sets.Operations;
using CRDT.UnitTestHelpers.TestTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

            var valueJson = JsonConvert.SerializeObject(value);

            pSet = pSet.Add(new Operation(JToken.Parse(valueJson)));

            Assert.Contains(value, pSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Add_Concurrent_AddsOnlyOneElement(TestType value)
        {
            var pSet = new P_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            pSet = pSet.Add(new Operation(JToken.Parse(valueJson)));
            pSet = pSet.Add(new Operation(JToken.Parse(valueJson)));

            Assert.Equal(1, pSet.Adds.Count(v => Equals(v, value)));
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(TestType value)
        {
            var pSet = new P_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            var newPSet = pSet.Remove(new Operation(JToken.Parse(valueJson)));

            Assert.Same(pSet, newPSet);
        }

        [Theory]
        [AutoData]
        public void Remove_AddsElementToRemovesSet(TestType value)
        {
            var pSet = new P_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            pSet = pSet.Add(new Operation(JToken.Parse(valueJson)));
            pSet = pSet.Remove(new Operation(JToken.Parse(valueJson)));

            Assert.Contains(value, pSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Remove_Concurrent_AddsOnlyOneElementToRemoveSet(TestType value)
        {
            var pSet = new P_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            pSet = pSet.Add(new Operation(JToken.Parse(valueJson)));
            pSet = pSet.Remove(new Operation(JToken.Parse(valueJson)));
            pSet = pSet.Remove(new Operation(JToken.Parse(valueJson)));

            Assert.Equal(1, pSet.Removes.Count(v => Equals(v, value)));
        }

        [Theory]
        [AutoData]
        public void Value_ReturnsAddedElementIfItWasNotRemoved(TestType value)
        {
            var pSet = new P_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            pSet = pSet.Add(new Operation(JToken.Parse(valueJson)));

            var actualValue = pSet.Value(value.Id);

            Assert.Equal(value, actualValue);
        }

        [Theory]
        [AutoData]
        public void Value_ReturnsNullIfItWasRemoved(TestType value)
        {
            var pSet = new P_Set<TestType>();

            var valueJson = JsonConvert.SerializeObject(value);

            pSet = pSet.Add(new Operation(JToken.Parse(valueJson)));
            pSet = pSet.Remove(new Operation(JToken.Parse(valueJson))); ;

            var actualValue = pSet.Value(value.Id);

            Assert.Null(actualValue);
        }

        [Theory]
        [AutoData]
        public void Merge_MergesAddsAndRemoves(TestType one, TestType two, TestType three, TestType four, TestType five)
        {
            var firstPSet = new P_Set<TestType>(new[] { one, two }.ToImmutableHashSet(), new[] { three }.ToImmutableHashSet());

            var secondPSet = new P_Set<TestType>(new[] { three, four }.ToImmutableHashSet(), new[] { five }.ToImmutableHashSet());

            var pSet = firstPSet.Merge(secondPSet);

            Assert.Equal(4, pSet.Adds.Count);
            Assert.Equal(2, pSet.Removes.Count);
            Assert.Contains(one, pSet.Adds);
            Assert.Contains(two, pSet.Adds);
            Assert.Contains(three, pSet.Adds);
            Assert.Contains(four, pSet.Adds);
            Assert.Contains(three, pSet.Removes);
            Assert.Contains(five, pSet.Removes);
            Assert.Contains(five, pSet.Removes);
        }
    }
}
