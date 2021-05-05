using System.Collections.Generic;
using System.Linq;
using Benchmarks.TestTypes;
using CRDT.Application.Interfaces;

namespace Benchmarks.Repositories
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