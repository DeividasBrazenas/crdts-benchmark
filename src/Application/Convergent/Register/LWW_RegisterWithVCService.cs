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
        private readonly object _lockObject = new();

        public LWW_RegisterWithVCService(ILWW_RegisterWithVCRepository<T> repository)
        {
            _repository = repository;
        }

        public void LocalAssign(T value, VectorClock vectorClock)
        {
            lock (_lockObject)
            {
                var existingEntity = _repository.GetElement(value.Id);

                LWW_RegisterWithVC<T> existingRegister;
                if (existingEntity is null)
                {
                    existingRegister = new LWW_RegisterWithVC<T>(new LWW_RegisterWithVCElement<T>(null, new VectorClock(), false));
                }
                else
                {
                    existingRegister = new LWW_RegisterWithVC<T>(existingEntity);
                }

                var newRegister = existingRegister.Assign(value, vectorClock);

                _repository.PersistElement(newRegister.Element);
            }
        }

        public void LocalRemove(T value, VectorClock vectorClock)
        {
            lock (_lockObject)
            {
                var existingEntity = _repository.GetElement(value.Id);

                LWW_RegisterWithVC<T> register;
                if (existingEntity is null)
                {
                    var element = new LWW_RegisterWithVCElement<T>(null, new VectorClock(), false);
                    register = new LWW_RegisterWithVC<T>(element);
                }
                else
                {
                    register = new LWW_RegisterWithVC<T>(existingEntity);
                }

                register = register.Remove(value, vectorClock);

                _repository.PersistElement(register.Element);
            }
        }

        public void DownstreamAssign(T value, VectorClock vectorClock)
        {
            lock (_lockObject)
            {
                var existingEntity = _repository.GetElement(value.Id);

                LWW_RegisterWithVC<T> existingRegister;
                if (existingEntity is null)
                {
                    existingRegister = new LWW_RegisterWithVC<T>(new LWW_RegisterWithVCElement<T>(null, new VectorClock(), false));
                }
                else
                {
                    existingRegister = new LWW_RegisterWithVC<T>(existingEntity);
                }

                var newRegister = existingRegister.Assign(value, vectorClock);

                _repository.PersistElement(newRegister.Element);
            }
        }

        public void DownstreamRemove(T value, VectorClock vectorClock)
        {
            lock (_lockObject)
            {
                var existingEntity = _repository.GetElement(value.Id);

                LWW_RegisterWithVC<T> register;
                if (existingEntity is null)
                {
                    var element = new LWW_RegisterWithVCElement<T>(null, new VectorClock(), false);
                    register = new LWW_RegisterWithVC<T>(element);
                }
                else
                {
                    register = new LWW_RegisterWithVC<T>(existingEntity);
                }

                register = register.Remove(value, vectorClock);

                _repository.PersistElement(register.Element);
            }
        }

        public LWW_RegisterWithVCElement<T> GetValue(Guid id) => _repository.GetElement(id);
    }
}