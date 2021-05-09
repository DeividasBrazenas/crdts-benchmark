using System;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Registers.Commutative.LastWriterWins;
using CRDT.Registers.Entities;
using Newtonsoft.Json.Linq;

namespace CRDT.Application.Commutative.Register
{
    public class LWW_RegisterWithVCService<T> where T : DistributedEntity
    {
        private readonly ILWW_RegisterWithVCRepository<T> _repository;
        private readonly object _lockObject = new();

        public LWW_RegisterWithVCService(ILWW_RegisterWithVCRepository<T> repository)
        {
            _repository = repository;
        }

        public void LocalAssign(Guid id, JToken value, VectorClock vectorClock)
        {
            lock (_lockObject)
            {
                var existingEntity = _repository.GetElement(id);

                LWW_RegisterWithVC<T> register;
                if (existingEntity is null)
                {
                    var element = new LWW_RegisterWithVCElement<T>(BaseObject(id), new VectorClock(), false);
                    register = new LWW_RegisterWithVC<T>(element);
                }
                else
                {
                    register = new LWW_RegisterWithVC<T>(existingEntity);
                }

                var newRegister = register.Assign(value, vectorClock);

                if (Equals(register, newRegister))
                {
                    return;
                }

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
                    var element = new LWW_RegisterWithVCElement<T>(BaseObject(value.Id), new VectorClock(), false);
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

        public void DownstreamAssign(Guid id, JToken value, VectorClock vectorClock)
        {
            lock (_lockObject)
            {
                var existingEntity = _repository.GetElement(id);

                LWW_RegisterWithVC<T> register;
                if (existingEntity is null)
                {
                    var element = new LWW_RegisterWithVCElement<T>(BaseObject(id), new VectorClock(), false);
                    register = new LWW_RegisterWithVC<T>(element);
                }
                else
                {
                    register = new LWW_RegisterWithVC<T>(existingEntity);
                }

                var newRegister = register.Assign(value, vectorClock);

                if (Equals(register, newRegister))
                {
                    return;
                }

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
                    var element = new LWW_RegisterWithVCElement<T>(BaseObject(value.Id), new VectorClock(), false);
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

        private T BaseObject(Guid id)
        {
            var obj = new JObject { ["Id"] = id };

            return obj.ToObject<T>();
        }
    }
}