using Abstractions.Entities;

namespace Abstractions.Interfaces
{
    public interface ICommutative<out T>
    {
        T Merge(Operation operation);
    }
}