using System;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Registers.Convergent.LastWriterWins;
using CRDT.Registers.Entities;

namespace CRDT.Application.Convergent.Register
{
    public class LWW_RegisterService<T> where T : DistributedEntity
    {
        private readonly ILWW_RegisterRepository<T> _repository;
        private readonly object _lockObject = new();

        public LWW_RegisterService(ILWW_RegisterRepository<T> repository)
        {
            _repository = repository;
        }

        public void LocalAssign(Guid id, T value, long timestamp)
        {
            lock (_lockObject)
            {
                var existingEntity = _repository.GetElement(id);

                LWW_Register<T> existingRegister;
                if (existingEntity is null)
                {
                    existingRegister = new LWW_Register<T>(new LWW_RegisterElement<T>(null, 0));
                }
                else
                {
                    existingRegister = new LWW_Register<T>(existingEntity);
                }

                var register = existingRegister.Assign(value, timestamp);

                _repository.PersistElement(register.Element);
            }
        }

        public void DownstreamAssign(Guid id, T value, long timestamp)
        {
            lock(_lockObject)
            {
                var existingEntity = _repository.GetElement(id);

                LWW_Register<T> existingRegister;
                if (existingEntity is null)
                {
                    existingRegister = new LWW_Register<T>(new LWW_RegisterElement<T>(null, 0));
                }
                else
                {
                    existingRegister = new LWW_Register<T>(existingEntity);
                }

                var register = existingRegister.Assign(value, timestamp);

                _repository.PersistElement(register.Element);
            }
        }

        public T GetValue(Guid id)
        {
            var entity = _repository.GetElement(id);

            return entity?.Value;
        }
    }
}