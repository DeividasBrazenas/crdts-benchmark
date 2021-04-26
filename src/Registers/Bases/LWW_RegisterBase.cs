using CRDT.Core.Abstractions;
using CRDT.Registers.Entities;

namespace CRDT.Registers.Bases
{
    public abstract class LWW_RegisterBase<T> : ValueObject where T : DistributedEntity
    {
        public LWW_RegisterElement<T> Element { get; }

        protected LWW_RegisterBase(LWW_RegisterElement<T> element)
        {
            Element = element;
        }
    }
}