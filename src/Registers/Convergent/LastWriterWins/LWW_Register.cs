using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
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
            if (Equals(Element.Value, value))
            {
                return this;
            }

            if (Element.Timestamp < new Timestamp(timestamp))
            {
                return new LWW_Register<T>(new LWW_RegisterElement<T>(value, timestamp));
            }

            return this;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Element;
        }
    }
}