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

        public void LocalAssign(T value, long timestamp)
        {
            lock (_lockObject)
            {
                var existingEntity = _repository.GetElement(value.Id);

                LWW_Register<T> existingRegister;
                if (existingEntity is null)
                {
                    existingRegister = new LWW_Register<T>(new LWW_RegisterElement<T>(null, 0, false));
                }
                else
                {
                    existingRegister = new LWW_Register<T>(existingEntity);
                }

                var register = existingRegister.Assign(value, timestamp);

                _repository.PersistElement(register.Element);
            }
        }

        public void LocalRemove(T value, long timestamp)
        {
            lock (_lockObject)
            {
                var existingEntity = _repository.GetElement(value.Id);

                LWW_Register<T> existingRegister;
                if (existingEntity is null)
                {
                    existingRegister = new LWW_Register<T>(new LWW_RegisterElement<T>(null, 0, false));
                }
                else
                {
                    existingRegister = new LWW_Register<T>(existingEntity);
                }

                var register = existingRegister.Remove(value, timestamp);

                _repository.PersistElement(register.Element);
            }
        }

        public void DownstreamAssign(T value, long timestamp)
        {
            lock(_lockObject)
            {
                var existingEntity = _repository.GetElement(value.Id);

                LWW_Register<T> existingRegister;
                if (existingEntity is null)
                {
                    existingRegister = new LWW_Register<T>(new LWW_RegisterElement<T>(null, 0, false));
                }
                else
                {
                    existingRegister = new LWW_Register<T>(existingEntity);
                }

                var register = existingRegister.Assign(value, timestamp);

                _repository.PersistElement(register.Element);
            }
        }

        public void DownstreamRemove(T value, long timestamp)
        {
            lock (_lockObject)
            {
                var existingEntity = _repository.GetElement(value.Id);

                LWW_Register<T> existingRegister;
                if (existingEntity is null)
                {
                    existingRegister = new LWW_Register<T>(new LWW_RegisterElement<T>(null, 0, false));
                }
                else
                {
                    existingRegister = new LWW_Register<T>(existingEntity);
                }

                var register = existingRegister.Remove(value, timestamp);

                _repository.PersistElement(register.Element);
            }
        }

        public LWW_RegisterElement<T> GetValue(Guid id) => _repository.GetElement(id);
    }
}