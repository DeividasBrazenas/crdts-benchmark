using System.Collections.Generic;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Application.UnitTests.Repositories
{
    public class LWW_OptimizedSetWithVCRepository : ILWW_OptimizedSetWithVCRepository<TestType>
    {
        public List<LWW_OptimizedSetWithVCElement<TestType>> Elements { get; }

        public LWW_OptimizedSetWithVCRepository()
        {
            Elements = new List<LWW_OptimizedSetWithVCElement<TestType>>();
        }

        public IEnumerable<LWW_OptimizedSetWithVCElement<TestType>> GetElements() => Elements;

        public void PersistElements(IEnumerable<LWW_OptimizedSetWithVCElement<TestType>> elements)
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