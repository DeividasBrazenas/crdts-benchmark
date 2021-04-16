using System;
using CRDT.Application.Entities;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Core.Cluster;
using CRDT.Registers.Convergent;

namespace CRDT.Application.Convergent
{
    public class LWW_RegisterService<T> where T : DistributedEntity
    {
        private readonly IRepository<T> _repository;

        public LWW_RegisterService(IRepository<T> repository)
        {
            _repository = repository;
        }

        public void Update(Guid id, T value, Node updatedBy, long timestamp)
        {
            var existingEntity = _repository.GetValue(id);

            if(existingEntity is null)
            {
                _repository.AddValue(new PersistenceEntity<T>(value, updatedBy, timestamp));

                return;
            }

            var existingRegister = new LWW_Register<T>(existingEntity.Value, existingEntity.UpdatedBy, existingEntity.Timestamp);

            var newRegister = new LWW_Register<T>(value, updatedBy, timestamp);

            var mergedRegister = existingRegister.Merge(newRegister);

            if(Equals(existingRegister, mergedRegister))
            {
                return;
            }

            _repository.ReplaceValue(id, new PersistenceEntity<T>(mergedRegister.Value, mergedRegister.UpdatedBy, mergedRegister.Timestamp));
        }

        public T GetValue(Guid id)
        {
            var entity = _repository.GetValue(id);

            return entity?.Value;
        }
    }
}