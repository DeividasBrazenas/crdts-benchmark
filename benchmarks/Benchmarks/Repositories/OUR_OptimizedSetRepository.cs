using System.Collections.Generic;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Benchmarks.Repositories
{
    public class OUR_OptimizedSetRepository : IOUR_OptimizedSetRepository<TestType>
    {
        public List<OUR_OptimizedSetElement<TestType>> Elements { get; }

        public OUR_OptimizedSetRepository()
        {
            Elements = new List<OUR_OptimizedSetElement<TestType>>();
        }

        public IEnumerable<OUR_OptimizedSetElement<TestType>> GetElements() => Elements;

        public void PersistElements(IEnumerable<OUR_OptimizedSetElement<TestType>> elements)
        {
            foreach (var element in elements)
            {
                var existingElement = Elements.FirstOrDefault(e => Equals(e.Value.Id, element.Value.Id) && e.Tag == element.Tag);

                if (existingElement is not null)
                {
                    Elements.Remove(existingElement);
                }

                Elements.Add(element);
            }
        }
    }
}