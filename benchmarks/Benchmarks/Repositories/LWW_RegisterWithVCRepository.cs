using System;
using System.Collections.Generic;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Registers.Entities;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Benchmarks.Repositories
{
    public class LWW_RegisterWithVCRepository : ILWW_RegisterWithVCRepository<TestType>
    {
        public List<LWW_RegisterWithVCElement<TestType>> Elements { get; }

        public LWW_RegisterWithVCRepository()
        {
            Elements = new List<LWW_RegisterWithVCElement<TestType>>();
        }

        public IEnumerable<LWW_RegisterWithVCElement<TestType>> GetElements() => Elements;

        public LWW_RegisterWithVCElement<TestType> GetElement(Guid id)
        {
            return Elements.FirstOrDefault(e => e.Value.Id == id);
        }

        public void PersistElement(LWW_RegisterWithVCElement<TestType> element)
        {
            var entity = Elements.FirstOrDefault(a => a.Value.Id == element.Value.Id);

            if (entity is not null)
            {
                Elements.Remove(entity);
            }

            Elements.Add(element);
        }
    }
}