using System;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Registers.Convergent;
using CRDT.Registers.Entities;

namespace CRDT.Application.Convergent
{
    public class LWW_RegisterService<T> where T : DistributedEntity
    {
        private readonly ILWW_RegisterRepository<T> _repository;

        public LWW_RegisterService(ILWW_RegisterRepository<T> repository)
        {
            _repository = repository;
        }

        public void Assign(Guid id, T value, long timestamp)
        {
            var existingEntity = _repository.GetElement(id);

            LWW_Register<T> existingRegister;
            if (existingEntity is null)
            {
                existingRegister = new LWW_Register<T>(new LWW_RegisterElement<T>(null, null));
            }
            else
            {
                existingRegister = new LWW_Register<T>(existingEntity);
            }

            var newRegister = existingRegister.Assign(value, timestamp);

            _repository.PersistElement(newRegister.Element);
        }

        public T GetValue(Guid id)
        {
            var entity = _repository.GetElement(id);

            return entity?.Value;
        }
    }
}