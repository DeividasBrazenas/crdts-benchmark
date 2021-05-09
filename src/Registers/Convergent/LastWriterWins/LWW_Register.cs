using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Registers.Entities;

namespace CRDT.Registers.Convergent.LastWriterWins
{
    public sealed class LWW_Register<T> : Bases.LWW_RegisterBase<T> where T : DistributedEntity
    {
        public LWW_Register(LWW_RegisterElement<T> element) : base(element)
        {
        }

        public LWW_Register<T> Assign(T value, long timestamp)
        {
            if (Element.Timestamp < timestamp)
            {
                return new LWW_Register<T>(new LWW_RegisterElement<T>(value, timestamp, false));
            }

            return this;
        }

        public LWW_Register<T> Remove(T value, long timestamp)
        {
            if (Element is null || Element.Value.Id != value.Id)
            {
                return this;
            }

            if (Element.Timestamp < timestamp)
            {
                return new LWW_Register<T>(new LWW_RegisterElement<T>(value, timestamp, true));
            }

            return this;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Element;
        }
    }
}