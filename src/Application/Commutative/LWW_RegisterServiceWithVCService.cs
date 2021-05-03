using System;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Registers.Commutative;
using CRDT.Registers.Entities;
using Newtonsoft.Json.Linq;

namespace CRDT.Application.Commutative
{
    public class LWW_RegisterWithVCService<T> where T : DistributedEntity
    {
        private readonly ILWW_RegisterWithVCRepository<T> _repository;

        public LWW_RegisterWithVCService(ILWW_RegisterWithVCRepository<T> repository)
        {
            _repository = repository;
        }

        public void Assign(Guid id, JToken value, VectorClock vectorClock)
        {
            var existingEntity = _repository.GetElement(id);

            LWW_RegisterWithVC<T> register; 
            if (existingEntity is null)
            {
                var element = new LWW_RegisterWithVCElement<T>(BaseObject(id), new VectorClock());
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