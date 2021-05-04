using System.Collections.Generic;
using System.Linq;
using Benchmarks.TestTypes;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;

namespace Benchmarks.Repositories
{
    public class OUR_SetRepository : IOUR_SetRepository<TestType>
    {
        public List<OUR_SetElement<TestType>> Adds { get; }
        public List<OUR_SetElement<TestType>> Removes { get; }

        public OUR_SetRepository()
        {
            Adds = new List<OUR_SetElement<TestType>>();
            Removes = new List<OUR_SetElement<TestType>>();
        }

        public IEnumerable<OUR_SetElement<TestType>> GetAdds() => Adds;

        public IEnumerable<OUR_SetElement<TestType>> GetRemoves() => Removes;

        public void PersistAdds(IEnumerable<OUR_SetElement<TestType>> values)
        {
            foreach (var value in values)
            {
                var entity = Adds.FirstOrDefault(a => a.Value.Id == value.Value.Id && a.Tag == value.Tag);

                if (entity is not null)
                {
                    Adds.Remove(entity);
                }

                Adds.Add(value);
            }
        }

        public void PersistRemoves(IEnumerable<OUR_SetElement<TestType>> values)
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