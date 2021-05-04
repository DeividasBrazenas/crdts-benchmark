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
                    var element = new LWW_RegisterElement<T>(BaseObject(id), null);
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

        public void DownstreamAssign(Guid id, JToken value, long timestamp)
        {
            lock(_lockObject)
            {
                var existingEntity = _repository.GetElement(id);

                LWW_Register<T> register;
                if (existingEntity is null)
                {
                    var element = new LWW_RegisterElement<T>(BaseObject(id), null);
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

        public T GetValue(Guid id)
        {
            var entity = _repository.GetElement(id);

            return entity?.Value;
        }

        private T BaseObject(Guid id)
        {
            var obj = new JObject { ["Id"] = id };

            return obj.ToObject<T>();
        }
    }
}