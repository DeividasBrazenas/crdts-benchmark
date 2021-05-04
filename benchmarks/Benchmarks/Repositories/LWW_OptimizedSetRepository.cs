using System.Collections.Generic;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Benchmarks.Repositories
{
    public class LWW_OptimizedSetRepository : ILWW_OptimizedSetRepository<TestType>
    {
        public List<LWW_OptimizedSetElement<TestType>> Elements { get; }

        public LWW_OptimizedSetRepository()
        {
            Elements = new List<LWW_OptimizedSetElement<TestType>>();
        }

        public IEnumerable<LWW_OptimizedSetElement<TestType>> GetElements() => Elements;

        public void PersistElements(IEnumerable<LWW_OptimizedSetElement<TestType>> elements)
        {
            foreach (var element in elements)
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
}