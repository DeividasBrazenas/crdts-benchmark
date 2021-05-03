using System.Collections.Generic;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Application.UnitTests.Repositories
{
    public class OUR_OptimizedSetWithVCRepository : IOUR_OptimizedSetWithVCRepository<TestType>
    {
        public List<OUR_OptimizedSetWithVCElement<TestType>> Elements { get; }

        public OUR_OptimizedSetWithVCRepository()
        {
            Elements = new List<OUR_OptimizedSetWithVCElement<TestType>>();
        }

        public IEnumerable<OUR_OptimizedSetWithVCElement<TestType>> GetElements() => Elements;

        public void PersistElements(IEnumerable<OUR_OptimizedSetWithVCElement<TestType>> elements)
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