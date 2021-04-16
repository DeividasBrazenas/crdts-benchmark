using System;
using System.Linq;
using AutoFixture;
using CRDT.Core.DistributedTime;
using CRDT.UnitTestHelpers.TestTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CRDT.Core.UnitTests
{
    public class OperationPOC
    {
        [Theory]
        [AutoData]
        public void Test(TestType value, long timestamp)
        {
            var firstOperation = $"{{\"Id\":\"{value.Id}\", \"IntValue\":999}}";
            var secondOperation = $"{{\"DecimalValue\":10.0}}";
            var thirdOperation = $"{{\"Id\":\"{value.Id}\", \"ObjectValue\":{JsonConvert.SerializeObject(value.ObjectValue)}}}";
            var fourthOperation = $"{{\"Id\":\"{value.GuidValue}\", \"ObjectValue\":{JsonConvert.SerializeObject(value.ObjectValue)}}}";

            var first = Operation.Parse(firstOperation, timestamp);
            var second = Operation.Parse(secondOperation, timestamp + 1);
            var third = Operation.Parse(thirdOperation, timestamp + 2);
            var fourth = Operation.Parse(fourthOperation, timestamp + 3);

            var ops = new[] { first, second, third, fourth };

            var requiredOps = ops.Where(o => o != null && o.ElementId == value.Id).OrderBy(o => o.Timestamp);

            var obj = new JObject();

            foreach (var req in requiredOps)
            {
                obj.Merge(req.Value);
            }

            var actual = obj.ToObject<TestType>();

            Assert.Equal(value.Id, actual.Id);
            Assert.Equal(999, actual.IntValue);
            Assert.Equal(value.ObjectValue, actual.ObjectValue);
        }
    }

    public class Operation
    {
        public Guid OperationId { get; }

        public Guid ElementId { get; }

        public Timestamp Timestamp { get; }

        public JToken Value { get; }

        public Operation(Guid elementId, JToken value, long timestamp)
        {
            OperationId = Guid.NewGuid();
            ElementId = elementId;
            Timestamp = new Timestamp(timestamp);
            Value = value;
        }

        public static Operation Parse(string valueJson, long timestamp)
        {
            var jToken = JToken.Parse(valueJson);
            var idToken = jToken["Id"];

            if (idToken is null)
            {
                return null;
            }

            return new Operation(idToken.ToObject<Guid>(), jToken, timestamp);
        }
    }
}