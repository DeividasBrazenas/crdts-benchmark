using CRDT.Core.Abstractions;
using CRDT.Registers.Entities;

namespace CRDT.Registers.Bases
{
    public abstract class LWW_RegisterWithVCBase<T> : ValueObject where T : DistributedEntity
    {
        public LWW_RegisterWithVCElement<T> Element { get; }

        protected LWW_RegisterWithVCBase(LWW_RegisterWithVCElement<T> element)
        {
            Element = element;
        }
    }
}