using System.Collections.Generic;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Benchmarks.Repositories
{
    public class G_SetRepository : IG_SetRepository<TestType>
    {
        public List<TestType> Entities { get; }

        public G_SetRepository()
        {
            Entities = new List<TestType>();
        }

        public IEnumerable<TestType> GetValues()
        {
            return Entities;
        }

        public void PersistValues(IEnumerable<TestType> values)
        {
            foreach (var value in values)
            {
                if (!Entities.Any(e => Equals(e, value)))
                {
                    Entities.Add(value);
                }
            }
        }
    }
}