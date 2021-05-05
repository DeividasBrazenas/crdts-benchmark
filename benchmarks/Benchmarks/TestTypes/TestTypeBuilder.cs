using System;
using System.Collections.Generic;

namespace Benchmarks.TestTypes
{
    public class TestTypeBuilder
    {
        private readonly Random _random;

        public TestTypeBuilder(Random random)
        {
            _random = random;
        }

        public TestType Build(Guid? guid = null)
        {
            if (guid is null)
            {
                guid = Guid.NewGuid();
            }

            var value = new TestType(guid.Value)
            {
                StringValue = Guid.NewGuid().ToString(),
                IntValue = _random.Next(),
                DecimalValue = _random.Next(),
                NullableLongValue = _random.Next(),
                GuidValue = Guid.NewGuid(),
                IntArray = new[] { _random.Next(), _random.Next(), _random.Next() },
                LongList = new List<long> { _random.Next(), _random.Next(), _random.Next() },
                ObjectValue = BuildInnerObject()
            };

            return value;
        }

        public List<TestType> Build(Guid? guid, int count)
        {
            var objects = new List<TestType>();

            for (int i = 0; i < count; i++)
            {
                objects.Add(Build(guid));
            }

            return objects;
        }

        private InnerTestType BuildInnerObject()
        {
            var value = new InnerTestType
            {
                DecimalValue = _random.Next(),
                IntValue = _random.Next(),
                NullableLongValue = _random.Next(),
                StringValue = Guid.NewGuid().ToString()
            };

            return value;
        }
    }
}