using System;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Registers.Commutative.LastWriterWins;
using CRDT.Registers.Entities;
using Newtonsoft.Json.Linq;

namespace CRDT.Application.Commutative.Register
{
    public class LWW_RegisterService<T> where T : DistributedEntity
    {
        private readonly ILWW_RegisterRepository<T> _repository;
        private readonly object _lockObject = new();

        public LWW_RegisterService(ILWW_RegisterRepository<T> repository)
        {
            _repository = repository;
        }

        public void LocalAssign(Guid id, JToken value, long timestamp)
        {
            lock (_lockObject)
            {
                var existingEntity = _repository.GetElement(id);

                LWW_Register<T> register;
                if (existingEntity is null)
                {
                    var element = new LWW_RegisterElement<T>(BaseObject(id), 0, false);
                    register = new LWW_Register<T>(element);
                }
                else
                {
                    register = new LWW_Register<T>(existingEntity);
                }

                register = register.Assign(value, timestamp);

                _repository.PersistElement(register.Element);
            }
        }

        public void LocalRemove(T value, long timestamp)
        {
            lock (_lockObject)
            {
                var existingEntity = _repository.GetElement(value.Id);

                LWW_Register<T> register;
                if (existingEntity is null)
                {
                    var element = new LWW_RegisterElement<T>(BaseObject(value.Id), 0, false);
                    register = new LWW_Register<T>(element);
                }
                else
                {
                    register = new LWW_Register<T>(existingEntity);
                }

                register = register.Remove(value, timestamp);

                _repository.PersistElement(register.Element);
            }
        }

        public void DownstreamAssign(Guid id, JToken value, long timestamp)
        {
            lock(_lockObject)
            {
                var existingEntity = _repository.GetElement(id);

                LWW_Register<T> register;
                if (existingEntity is null)
                {
                    var element = new LWW_RegisterElement<T>(BaseObject(id), 0, false);
                    register = new LWW_Register<T>(element);
                }
                else
                {
                    register = new LWW_Register<T>(existingEntity);
                }

                register = register.Assign(value, timestamp);

                _repository.PersistElement(register.Element);
            }
        }

        public void DownstreamRemove(T value, long timestamp)
        {
            lock (_lockObject)
            {
                var existingEntity = _repository.GetElement(value.Id);

                LWW_Register<T> register;
                if (existingEntity is null)
                {
                    var element = new LWW_RegisterElement<T>(BaseObject(value.Id), 0, false);
                    register = new LWW_Register<T>(element);
                }
                else
                {
                    register = new LWW_Register<T>(existingEntity);
                }

                register = register.Remove(value, timestamp);

                _repository.PersistElement(register.Element);
            }
        }

        public LWW_RegisterElement<T> GetValue(Guid id) => _repository.GetElement(id);

        private T BaseObject(Guid id)
        {
            var obj = new JObject { ["Id"] = id };

            return obj.ToObject<T>();
        }
    }
}