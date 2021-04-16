using System;
using CRDT.Application.Entities;
using CRDT.Application.Interfaces;
using CRDT.Core.Abstractions;
using CRDT.Registers.Commutative;
using CRDT.Registers.Operations;
using Newtonsoft.Json.Linq;

namespace CRDT.Application.Commutative
{
    public class LWW_RegisterService<T> where T : DistributedEntity
    {
        private readonly IRepository<T> _repository;

        public LWW_RegisterService(IRepository<T> repository)
        {
            _repository = repository;
        }

        public void Update(Guid id, Operation operation)
        {
            var existingEntity = _repository.GetValue(id);

            LWW_Register<T> register; 
            if (existingEntity is null)
            {
                register = new LWW_Register<T>(BaseObject(id).ToObject<T>(), null, null);
            }
            else
            {
                register = new LWW_Register<T>(existingEntity.Value, existingEntity.UpdatedBy, existingEntity.Timestamp);
            }

            var newRegister = register.Update(operation);

            if (Equals(register, newRegister))
            {
                return;
            }

            _repository.ReplaceValue(id,
                new PersistenceEntity<T>(newRegister.Value, newRegister.UpdatedBy, newRegister.Timestamp));
        }

        public T GetValue(Guid id)
        {
            var entity = _repository.GetValue(id);

            return entity?.Value;
        }

        private JObject BaseObject(Guid id)
        {
            var obj = new JObject { ["Id"] = id };

            return obj;
        }
    }
}