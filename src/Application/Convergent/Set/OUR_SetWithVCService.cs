using System.Collections.Generic;
using System.Collections.Immutable;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Sets.Convergent.ObservedUpdatedRemoved;
using CRDT.Sets.Entities;

namespace CRDT.Application.Convergent.Set
{
    public class OUR_SetWithVCService<T> where T : DistributedEntity
    {
        private readonly IOUR_SetWithVCRepository<T> _repository;

        public OUR_SetWithVCService(IOUR_SetWithVCRepository<T> repository)
        {
            _repository = repository;
        }

        public void Merge(IEnumerable<OUR_SetWithVCElement<T>> adds, IEnumerable<OUR_SetWithVCElement<T>> removes)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OUR_SetWithVC<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            set = set.Merge(adds.ToImmutableHashSet(), removes.ToImmutableHashSet());

            _repository.PersistAdds(set.Adds);
            _repository.PersistRemoves(set.Removes);
        }

        public bool Lookup(T value)
        {
            var existingAdds = _repository.GetAdds();
            var existingRemoves = _repository.GetRemoves();

            var set = new OUR_SetWithVC<T>(existingAdds.ToImmutableHashSet(), existingRemoves.ToImmutableHashSet());

            var lookup = set.Lookup(value);

            return lookup;
        }
    }
}