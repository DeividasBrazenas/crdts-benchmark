using System.Collections.Generic;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Application.UnitTests.Repositories
{
    public class P_SetRepository : IP_SetRepository<TestType>
    {
        public List<TestType> Adds { get; }
        public List<TestType> Removes { get; }

        public P_SetRepository()
        {
            Adds = new List<TestType>();
            Removes = new List<TestType>();
        }

        public IEnumerable<TestType> GetAdds() => Adds;

        public IEnumerable<TestType> GetRemoves() => Removes;

        public void PersistAdds(IEnumerable<TestType> values)
        {
            foreach (var value in values)
            {
                if (!Adds.Any(e => Equals(e, value)))
                {
                    Adds.Add(value);
                }
            }
        }

        public void PersistRemoves(IEnumerable<TestType> values)
        {
            foreach (var value in values)
            {
                if (!Removes.Any(e => Equals(e, value)))
                {
                    Removes.Add(value);
                }
            }
        }
    }
}