using System.Collections.Generic;
using CRDT.Core.Abstractions;
using CRDT.Core.DistributedTime;
using CRDT.Registers.Entities;

namespace CRDT.Registers.Convergent.LastWriterWins
{
    public sealed class LWW_RegisterWithVC<T> : Bases.LWW_RegisterWithVCBase<T> where T : DistributedEntity
    {
        public LWW_RegisterWithVC(LWW_RegisterWithVCElement<T> element) : base(element)
        {
        }

        public LWW_RegisterWithVC<T> Assign(T value, VectorClock vectorClock)
        {
            if (Equals(Element.Value, value))
            {
                return this;
            }

            if (Element.VectorClock < vectorClock)
            {
                return new LWW_RegisterWithVC<T>(new LWW_RegisterWithVCElement<T>(value, vectorClock));
            }

            return this;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Element;
        }
    }
}