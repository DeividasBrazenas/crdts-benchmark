using System.Collections.Generic;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Benchmarks.Repositories
{
    public class LWW_SetWithVCRepository : ILWW_SetWithVCRepository<TestType>
    {
        public List<LWW_SetWithVCElement<TestType>> Adds { get; }
        public List<LWW_SetWithVCElement<TestType>> Removes { get; }

        public LWW_SetWithVCRepository()
        {
            Adds = new List<LWW_SetWithVCElement<TestType>>();
            Removes = new List<LWW_SetWithVCElement<TestType>>();
        }

        public IEnumerable<LWW_SetWithVCElement<TestType>> GetAdds() => Adds;

        public IEnumerable<LWW_SetWithVCElement<TestType>> GetRemoves() => Removes;

        public void PersistAdds(IEnumerable<LWW_SetWithVCElement<TestType>> values)
        {
            foreach (var value in values)
            {
                var entity = Adds.FirstOrDefault(a => a.Value.Id == value.Value.Id);

                if (entity is not null)
                {
                    Adds.Remove(entity);
                }

                Adds.Add(value);
            }
        }

        public void PersistRemoves(IEnumerable<LWW_SetWithVCElement<TestType>> values)
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