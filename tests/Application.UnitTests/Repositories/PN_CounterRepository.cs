using System.Collections.Generic;
using System.Linq;
using CRDT.Application.Interfaces;
using CRDT.Counters.Entities;

namespace CRDT.Application.UnitTests.Repositories
{
    public class PN_CounterRepository : IPN_CounterRepository
    {
        public List<CounterElement> Additions;

        public List<CounterElement> Subtractions;

        public PN_CounterRepository()
        {
            Additions = new List<CounterElement>();
            Subtractions = new List<CounterElement>();
        }

        public IEnumerable<CounterElement> GetAdditions()
        {
            return Additions;
        }

        public IEnumerable<CounterElement> GetSubtractions()
        {
            return Subtractions;
        }

        public void PersistAdditions(IEnumerable<CounterElement> values)
        {
            foreach (var value in values)
            {
                var existingValue = Additions.FirstOrDefault(v => v.Node.Id == value.Node.Id);

                if (existingValue is not null)
                {
                    Additions.Remove(existingValue);
                }

                Additions.Add(value);
            }
        }

        public void PersistSubtractions(IEnumerable<CounterElement> values)
        {
            foreach (var value in values)
            {
                var existingValue = Subtractions.FirstOrDefault(v => v.Node.Id == value.Node.Id);

                if (existingValue is not null)
                {
                    Subtractions.Remove(existingValue);
                }

                Subtractions.Add(value);
            }
        }
    }
}