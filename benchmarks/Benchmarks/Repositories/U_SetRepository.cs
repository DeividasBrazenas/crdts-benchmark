using System.Collections.Generic;
using System.Linq;
using Benchmarks.TestTypes;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;

namespace Benchmarks.Repositories
{
    public class U_SetRepository : IU_SetRepository<TestType>
    {
        public List<U_SetElement<TestType>> Elements { get; }

        public U_SetRepository()
        {
            Elements = new List<U_SetElement<TestType>>();
        }

        public IEnumerable<U_SetElement<TestType>> GetElements() => Elements;

        public void PersistElements(IEnumerable<U_SetElement<TestType>> values)
        {
            foreach (var value in values)
            {
                if (value.Removed)
                {
                    var add = Elements.FirstOrDefault(e => Equals(e.Value, value.Value) && !e.Removed);

                    if (add is not null)
                    {
                        Elements.Remove(add);
                    }
                }

                if (!Elements.Any(e => Equals(e, value)))
                {
                    Elements.Add(value);
                }
            }
        }
    }
}