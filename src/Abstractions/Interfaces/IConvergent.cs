namespace Abstractions.Interfaces
{
    public interface IConvergent<T>
    {
        T Merge(T other);
    }
}