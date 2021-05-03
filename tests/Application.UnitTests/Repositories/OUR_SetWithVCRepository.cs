using System.Collections.Generic;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Application.UnitTests.Repositories
{
    public class OUR_SetWithVCRepository : IOUR_SetWithVCRepository<TestType>
    {
        public List<OUR_SetWithVCElement<TestType>> Adds { get; }
        public List<OUR_SetWithVCElement<TestType>> Removes { get; }

        public OUR_SetWithVCRepository()
        {
            Adds = new List<OUR_SetWithVCElement<TestType>>();
            Removes = new List<OUR_SetWithVCElement<TestType>>();
        }

        public IEnumerable<OUR_SetWithVCElement<TestType>> GetAdds() => Adds;

        public IEnumerable<OUR_SetWithVCElement<TestType>> GetRemoves() => Removes;

        public void PersistAdds(IEnumerable<OUR_SetWithVCElement<TestType>> values)
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

        public void PersistRemoves(IEnumerable<OUR_SetWithVCElement<TestType>> values)
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