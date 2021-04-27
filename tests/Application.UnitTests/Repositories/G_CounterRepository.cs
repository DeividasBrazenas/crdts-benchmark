using System.Collections.Generic;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Counters.Entities;

namespace CRDT.Application.UnitTests.Repositories
{
    public class G_CounterRepository : IG_CounterRepository
    {
        public List<CounterElement> Elements;

        public G_CounterRepository()
        {
            Elements = new List<CounterElement>();
        }

        public IEnumerable<CounterElement> GetValues()
        {
            return Elements;
        }

        public void PersistValues(IEnumerable<CounterElement> values)
        {
            foreach (var value in values)
            {
                var existingValue = Elements.FirstOrDefault(v => v.Node.Id == value.Node.Id);

                if(existingValue is not null)
                {
                    Elements.Remove(existingValue);
                }

                Elements.Add(value);
            }
        }
    }
}