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

        public LWW_RegisterService(ILWW_RegisterRepository<T> repository)
        {
            _repository = repository;
        }

        public void Assign(Guid id, JToken value, long timestamp)
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

            var newRegister = register.Assign(value, timestamp);

            if (Equals(register, newRegister))
            {
                return;
            }

            _repository.PersistElement(newRegister.Element);
        }

        public T Value(Guid id)
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