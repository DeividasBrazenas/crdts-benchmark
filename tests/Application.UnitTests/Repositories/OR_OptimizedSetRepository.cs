using System.Collections.Generic;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;
using CRDT.UnitTestHelpers.TestTypes;

namespace CRDT.Application.UnitTests.Repositories
{
    public class OR_OptimizedSetRepository : IOR_OptimizedSetRepository<TestType>
    {
        public List<OR_OptimizedSetElement<TestType>> Elements { get; }

        public OR_OptimizedSetRepository()
        {
            Elements = new List<OR_OptimizedSetElement<TestType>>();
        }

        public IEnumerable<OR_OptimizedSetElement<TestType>> GetElements() => Elements;

        public void PersistElements(IEnumerable<OR_OptimizedSetElement<TestType>> elements)
        {
            foreach (var element in elements)
            {
                var existingElement = Elements.FirstOrDefault(e =>
                    Equals(e.Value, element.Value) && e.Tag == element.Tag);

                if (existingElement is not null)
                {
                    Elements.Remove(existingElement);
                }

                Elements.Add(element);
            }
        }
    }
}