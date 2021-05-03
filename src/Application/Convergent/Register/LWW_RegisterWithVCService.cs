using System;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Registers.Convergent.LastWriterWins;
using CRDT.Registers.Entities;

namespace CRDT.Application.Convergent.Register
{
    public class LWW_RegisterWithVCService<T> where T : DistributedEntity
    {
        private readonly ILWW_RegisterWithVCRepository<T> _repository;

        public LWW_RegisterWithVCService(ILWW_RegisterWithVCRepository<T> repository)
        {
            _repository = repository;
        }

        public void Assign(Guid id, T value, VectorClock vectorClock)
        {
            var existingEntity = _repository.GetElement(id);

            LWW_RegisterWithVC<T> existingRegister;
            if (existingEntity is null)
            {
                existingRegister = new LWW_RegisterWithVC<T>(new LWW_RegisterWithVCElement<T>(null, new VectorClock()));
            }
            else
            {
                existingRegister = new LWW_RegisterWithVC<T>(existingEntity);
            }

            var newRegister = existingRegister.Assign(value, vectorClock);

            _repository.PersistElement(newRegister.Element);
        }

        public T GetValue(Guid id)
        {
            var entity = _repository.GetElement(id);

            return entity?.Value;
        }
    }
}