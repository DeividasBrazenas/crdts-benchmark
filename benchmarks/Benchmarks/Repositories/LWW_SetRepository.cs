﻿using System.Collections.Generic;
using System.Linq;
using Benchmarks.TestTypes;
using CRDT.Application.Interfaces;
using CRDT.Sets.Entities;

namespace Benchmarks.Repositories
{
    public class LWW_SetRepository : ILWW_SetRepository<TestType>
    {
        public List<LWW_SetElement<TestType>> Adds { get; }
        public List<LWW_SetElement<TestType>> Removes { get; }

        public LWW_SetRepository()
        {
            Adds = new List<LWW_SetElement<TestType>>();
            Removes = new List<LWW_SetElement<TestType>>();
        }

        public IEnumerable<LWW_SetElement<TestType>> GetAdds() => Adds;

        public IEnumerable<LWW_SetElement<TestType>> GetRemoves() => Removes;

        public void PersistAdds(IEnumerable<LWW_SetElement<TestType>> values)
        {
            foreach (var value in values)
            {
                var entity = Adds.FirstOrDefault(a => a.Value.Id == value.Value.Id);

                if (entity is not null)
                {
                    Adds.Remove(entity);
                }

                Adds.Add(value);
            }
        }

        public void PersistRemoves(IEnumerable<LWW_SetElement<TestType>> values)
        {
            foreach (var value in values)
            {
                if (!Removes.Any(e => Equals(e, value)))
                {
                    Removes.Add(value);
                }
            }
        }
    }
}