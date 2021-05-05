using System;
using System.Collections.Generic;
using AutoFixture;

namespace CRDT.UnitTestHelpers.TestTypes
{
    public static class TestTypeBuilder
    {
        public static TestType Build(Guid? guid = null)
        {
            if (guid is null)
            {
                guid = Guid.NewGuid();
            }

            var value = new TestType(guid.Value);

            var fixture = new Fixture();

            value.StringValue = fixture.Create<string>();
            value.IntValue = fixture.Create<int>();
            value.DecimalValue = fixture.Create<decimal>();
            value.NullableLongValue = fixture.Create<long?>();
            value.GuidValue = fixture.Create<Guid?>();
            value.IntArray = fixture.Create<int[]>();
            value.LongList = fixture.Create<List<long>>();
            value.ObjectValue = fixture.Create<InnerTestType>();

            return value;
        }

        public static List<TestType> Build(Guid? guid, int count)
        {
            var objects = new List<TestType>();

            for (int i = 0; i < count; i++)
            {
                objects.Add(Build(guid));
            }

            return objects;
        }
    }
}