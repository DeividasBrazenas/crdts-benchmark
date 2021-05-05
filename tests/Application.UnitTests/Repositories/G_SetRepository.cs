using System.Collections.Generic;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Application.UnitTests.Repositories
{
    public class G_SetRepository : IG_SetRepository<TestType>
    {
        public IEnumerable<TestType> Elements { get; private set; }

        public G_SetRepository()
        {
            Elements = new List<TestType>();
        }

        public IEnumerable<TestType> GetValues()
        {
            return Elements;
        }

        public void PersistValues(IEnumerable<TestType> values)
        {
            Elements = values;
        }
    }
}