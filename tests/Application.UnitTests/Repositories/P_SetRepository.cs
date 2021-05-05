using System.Collections.Generic;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Application.UnitTests.Repositories
{
    public class P_SetRepository : IP_SetRepository<TestType>
    {
        public IEnumerable<TestType> Adds { get; private set; }
        public IEnumerable<TestType> Removes { get; private set; }

        public P_SetRepository()
        {
            Adds = new List<TestType>();
            Removes = new List<TestType>();
        }

        public IEnumerable<TestType> GetAdds() => Adds;

        public IEnumerable<TestType> GetRemoves() => Removes;

        public void PersistAdds(IEnumerable<TestType> values)
        {
            Adds = values;
        }

        public void PersistRemoves(IEnumerable<TestType> values)
        {
            Removes = values;
        }
    }
}