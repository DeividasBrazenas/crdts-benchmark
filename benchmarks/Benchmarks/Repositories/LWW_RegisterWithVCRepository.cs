using System;
using System.Collections.Generic;
using System.Linq;
using Benchmarks.TestTypes;
using CRDT.Application.Interfaces;
using CRDT.Registers.Entities;

namespace Benchmarks.Repositories
{
    public class LWW_RegisterWithVCRepository : ILWW_RegisterWithVCRepository<TestType>
    {
        public List<LWW_RegisterWithVCElement<TestType>> Elements { get; private set; }

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
            var elements = Elements.Where(e => e.Value.Id != element.Value.Id).ToList();

            elements.Add(element);

            Elements = elements;
        }
    }
}