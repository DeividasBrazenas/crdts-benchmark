using System;
using System.Collections.Generic;
using System.Linq;
using Benchmarks.TestTypes;
using CRDT.Application.Interfaces;
using CRDT.Registers.Entities;

namespace Benchmarks.Repositories
{
    public class LWW_RegisterRepository : ILWW_RegisterRepository<TestType>
    {
        public List<LWW_RegisterElement<TestType>> Elements { get; }

        public LWW_RegisterRepository()
        {
            Elements = new List<LWW_RegisterElement<TestType>>();
        }

        public IEnumerable<LWW_RegisterElement<TestType>> GetElements() => Elements;

        public LWW_RegisterElement<TestType> GetElement(Guid id)
        {
            return Elements.FirstOrDefault(e => e.Value.Id == id);
        }

        public void PersistElement(LWW_RegisterElement<TestType> element)
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