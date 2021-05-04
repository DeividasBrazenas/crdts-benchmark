using System.Collections.Generic;
using System.Linq;
using Benchmarks.TestTypes;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;

namespace Benchmarks.Repositories
{
    public class OR_SetRepository : IOR_SetRepository<TestType>
    {
        public List<OR_SetElement<TestType>> Adds { get; }
        public List<OR_SetElement<TestType>> Removes { get; }

        public OR_SetRepository()
        {
            Adds = new List<OR_SetElement<TestType>>();
            Removes = new List<OR_SetElement<TestType>>();
        }

        public IEnumerable<OR_SetElement<TestType>> GetAdds() => Adds;

        public IEnumerable<OR_SetElement<TestType>> GetRemoves() => Removes;

        public void PersistAdds(IEnumerable<OR_SetElement<TestType>> values)
        {
            foreach (var value in values)
            {
                if (!Adds.Any(e => Equals(e, value)))
                {
                    Adds.Add(value);
                }
            }
        }

        public void PersistRemoves(IEnumerable<OR_SetElement<TestType>> values)
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