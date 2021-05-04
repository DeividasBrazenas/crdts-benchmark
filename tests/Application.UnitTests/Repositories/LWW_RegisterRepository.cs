using System;
using System.Collections.Generic;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Registers.Entities;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Application.UnitTests.Repositories
{
    public class LWW_RegisterRepository : ILWW_RegisterRepository<TestType>
    {
        public List<LWW_RegisterElement<TestType>> Elements { get; private set; }

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
            var elements = Elements.Where(e => e.Value.Id != element.Value.Id).ToList();

            elements.Add(element);

            Elements = elements;
        }
    }
}