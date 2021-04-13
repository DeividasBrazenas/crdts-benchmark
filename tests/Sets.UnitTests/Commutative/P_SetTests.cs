using System.Collections.Generic;
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
        public void Add_AddsElementToAddsSet(TestType[] adds, TestType value)
        {
            var pSet = new P_Set<TestType>(adds.ToImmutableHashSet(), ImmutableHashSet<TestType>.Empty);

            var valueJson = JsonConvert.SerializeObject(value);

            pSet = pSet.Add(new Operation(JToken.Parse(valueJson)));

            Assert.Contains(value, pSet.Adds);
        }

        [Theory]
        [AutoData]
        public void Remove_BeforeAdd_HasNoEffect(TestType[] removes, TestType value)
        {
            var pSet = new P_Set<TestType>(ImmutableHashSet<TestType>.Empty, removes.ToImmutableHashSet());

            var valueJson = JsonConvert.SerializeObject(value);

            var newPSet = pSet.Remove(new Operation(JToken.Parse(valueJson)));

            Assert.Same(pSet, newPSet);
        }

        [Theory]
        [AutoData]
        public void Remove_AddsElementToRemovesSet(TestType[] removes, TestType value)
        {
            var pSet = new P_Set<TestType>(ImmutableHashSet<TestType>.Empty, removes.ToImmutableHashSet());

            var valueJson = JsonConvert.SerializeObject(value);

            pSet = pSet.Add(new Operation(JToken.Parse(valueJson)));
            pSet = pSet.Remove(new Operation(JToken.Parse(valueJson)));

            Assert.Contains(value, pSet.Removes);
        }

        [Theory]
        [AutoData]
        public void Value_ReturnsAddedElementIfItWasNotRemoved(TestType[] adds, TestType value)
        {
            var pSet = new P_Set<TestType>(adds.ToImmutableHashSet(), ImmutableHashSet<TestType>.Empty);

            var valueJson = JsonConvert.SerializeObject(value);

            pSet = pSet.Add(new Operation(JToken.Parse(valueJson)));

            var actualValue = pSet.Value(value.Id);

            Assert.Equal(value, actualValue);
        }

        [Theory]
        [AutoData]
        public void Value_ReturnsNullIfItWasRemoved(TestType[] adds, TestType value)
        {
            var pSet = new P_Set<TestType>(adds.ToImmutableHashSet(), ImmutableHashSet<TestType>.Empty);

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
