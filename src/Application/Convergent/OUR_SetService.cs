using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Convergent;
using CRDT.Sets.Entities;

namespace CRDT.Application.Convergent
{
    public class OUR_SetService<T> where T : DistributedEntity
    {
        private readonly IOUR_SetRepository<T> _repository;

        public OUR_SetService(IOUR_SetRepository<T> repository)
        {
            _repository = repository;
        }

        public void Merge(IEnumerable<OUR_SetElement<T>> adds, IEnumerable<OUR_SetElement<T>> removes)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OUR_Set<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            set = set.Merge(adds.ToImmutableHashSet(), removes.ToImmutableHashSet());

            _repository.PersistAdds(set.Adds);
            _repository.PersistRemoves(set.Removes);
        }

        public bool Lookup(T value)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OUR_Set<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            var lookup = set.Lookup(value);

            return lookup;
        }
    }
}